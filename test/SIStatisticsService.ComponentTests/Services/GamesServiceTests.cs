using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.ComponentTests.Services;

internal sealed class GamesServiceTests : TestsBase
{
    [TestCase(null)]
    [TestCase("en")]
    public async Task AddGameResultAsync_GetGame_Ok(string? languageCode)
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }, ""))
        {
            Name = $"Test game {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now,
            LanguageCode = languageCode
        };

        var gameId = await GamesService.AddGameResultAsync(gameResult);
        var game = await GamesService.TryGetGameAsync(gameId);

        Assert.That(game, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(game.Name, Is.EqualTo(gameResult.Name));
            Assert.That(game.LanguageCode, Is.EqualTo(gameResult.LanguageCode));
            Assert.That(game.Package.Name, Is.EqualTo(gameResult.Package.Name));
        });
    }

    [Test]
    public async Task AddGameResultAsync_HandleDuplicates_Ok()
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }))
        {
            Name = $"Test game {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult);
        Assert.DoesNotThrowAsync(() => GamesService.AddGameResultAsync(gameResult));
    }

    [Test]
    public async Task AddGameResultAsync_PackageWithAuthorContacts_UpdateContacts()
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }))
        {
            Name = $"Test game {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now
        };

        var gameId = await GamesService.AddGameResultAsync(gameResult);

        var game = await GamesService.TryGetGameAsync(gameId);
        Assert.That(game, Is.Not.Null);
        Assert.That(game.Package.AuthorsContacts, Is.Null);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }, "Contact value"))
        {
            Name = $"Test game 2 {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now
        };

        Assert.DoesNotThrowAsync(() => GamesService.AddGameResultAsync(gameResult2));

        game = await GamesService.TryGetGameAsync(gameId);
        Assert.That(game, Is.Not.Null);
        Assert.That(game.Package.AuthorsContacts, Is.EqualTo("Contact value"));

        var gameResult3 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }))
        {
            Name = $"Test game {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now
        };

        var gameId3 = await GamesService.AddGameResultAsync(gameResult3);

        var game3 = await GamesService.TryGetGameAsync(gameId3);
        Assert.That(game3, Is.Not.Null);
        Assert.That(game3.Package.AuthorsContacts, Is.EqualTo("Contact value"));
    }

    [TestCase(1)]
    [TestCase(10)]
    public async Task GetGamesByFilter_CountFilter_Ok(int count)
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }))
        {
            Name = $"Test game {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }, "Contact value"))
        {
            Name = $"Test game 2 {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult2);

        var games = await GamesService.GetGamesByFilterAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)),
                To = DateTimeOffset.Now,
                Platform = GamePlatforms.Local,
                Count = count
            });

        Assert.That(games, count == 1 ? Has.Length.EqualTo(count) : Has.Length.GreaterThan(1));
    }

    [Test]
    public async Task GetGamesByFilter_LanguageFilter_Ok()
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }))
        {
            Name = $"Test game {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now,
            LanguageCode = "en"
        };

        await GamesService.AddGameResultAsync(gameResult);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }, "Contact value"))
        {
            Name = $"Test game 2 {randomId}",
            Results = new(),
            Reviews = new(),
            FinishTime = DateTimeOffset.Now,
            LanguageCode = "en"
        };

        await GamesService.AddGameResultAsync(gameResult2);

        var games = await GamesService.GetGamesByFilterAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)),
                To = DateTimeOffset.Now,
                Platform = GamePlatforms.Local,
                LanguageCode = "en"
            });

        Assert.That(games.Select(g => g.LanguageCode), Is.All.EqualTo("en"));
    }

    [TestCase(1)]
    [TestCase(10)]
    public async Task GetPackagesStatistic_CountFilter_Ok(int count)
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package 2 {randomId}", "", new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }, "Contact value"))
        {
            Name = $"Test game 2 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult2);

        var packages = await GamesService.GetPackagesStatisticAsync(
            new TopPackagesRequest(new StatisticFilter
            {
                From = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)),
                To = DateTimeOffset.Now,
                Platform = GamePlatforms.Local,
                Count = count
            }));

        Assert.That(packages.Packages, count == 1 ? Has.Length.EqualTo(count) : Has.Length.GreaterThan(1));
    }
}
