using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.ComponentTests.Services;

internal sealed class GamesServicePackageInfoTests : TestsBase
{
    [Test]
    public async Task GetPackageInfoAsync_NonExistent_ReturnsNull()
    {
        var request = new PackageInfoRequest("NonExistent", "nohash", new[] { "NoAuthor" });
        var result = await GamesService.GetPackageInfoAsync(request);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPackageInfoAsync_ReturnsPackageInfo_NoStats()
    {
        var randomId = Guid.NewGuid();
        var testTime = DateTimeOffset.Now.AddHours(-1);

        var packageName = $"Pkg_NoStats_{randomId}";
        var packageHash = "hash_ns";
        var authors = new[] { $"Author_{randomId}" };

        var gameResult = new GameResultInfo(new PackageInfo(packageName, packageHash, authors))
        {
            Name = $"Game_{randomId}",
            FinishTime = testTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var request = new PackageInfoRequest(packageName, packageHash, authors, IncludeStats: false);
        var info = await GamesService.GetPackageInfoAsync(request);

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.Package.Name, Is.EqualTo(packageName));
        Assert.That(info.Stats, Is.Null);
    }

    [Test]
    public async Task GetPackageInfoAsync_ReturnsPackageInfo_WithStats()
    {
        var randomId = Guid.NewGuid();
        var testTime = DateTimeOffset.Now.AddHours(-2);

        var packageName = $"Pkg_WithStats_{randomId}";
        var packageHash = "hash_ws";
        var authors = new[] { $"Author_{randomId}" };

        var packageStats = new PackageStats(
            new PackageTopLevelStats(3, 2),
            new Dictionary<string, QuestionStats>
            {
                ["q1"] = new QuestionStats(10, 9, 5, 4, 6)
            }
        );

        var gameResult = new GameResultInfo(new PackageInfo(packageName, packageHash, authors))
        {
            Name = $"GameStats_{randomId}",
            FinishTime = testTime
        };

        await GamesService.AddGameResultAsync(gameResult, packageStats);

        var request = new PackageInfoRequest(packageName, packageHash, authors, IncludeStats: true);
        var info = await GamesService.GetPackageInfoAsync(request);

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.Package.Name, Is.EqualTo(packageName));
        Assert.That(info.Stats, Is.Not.Null);
        Assert.That(info.Stats!.TopLevelStats.CompletedGameCount, Is.EqualTo(2));
        Assert.That(info.Stats.QuestionStats, Contains.Key("q1"));
    }

    [Test]
    public async Task GetPackageInfoAsync_SourceSelection_PicksRequestedSource()
    {
        var randomId = Guid.NewGuid();
        var testTime = DateTimeOffset.Now.AddHours(-3);

        var packageName = $"Pkg_Src_{randomId}";
        var packageHash = "hash_src";
        var authors = new[] { $"Author_{randomId}" };

        var source1 = new Uri("https://source1.example.com/packages");
        var source2 = new Uri("https://source2.example.com/packages");

        var game1 = new GameResultInfo(new PackageInfo(packageName, packageHash, authors, null, source1))
        {
            Name = $"Game1_{randomId}",
            FinishTime = testTime
        };

        var game2 = new GameResultInfo(new PackageInfo(packageName, packageHash, authors, null, source2))
        {
            Name = $"Game2_{randomId}",
            FinishTime = testTime.AddMinutes(1)
        };

        await GamesService.AddGameResultAsync(game1);
        await GamesService.AddGameResultAsync(game2);

        // Request info preferring source2
        var request = new PackageInfoRequest(packageName, packageHash, authors, source2, IncludeStats: false);
        var info = await GamesService.GetPackageInfoAsync(request);

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.Package.Source, Is.EqualTo(source2));
    }
}
