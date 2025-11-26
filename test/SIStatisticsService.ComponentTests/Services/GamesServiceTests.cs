using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.ComponentTests.Services;

internal sealed class GamesServiceTests : TestsBase
{
    [TestCase(null)]
    [TestCase("en")]
    public async Task AddGameResultAsync_GetGame_Ok(string? languageCode)
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"], ""))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now,
            LanguageCode = languageCode
        };

        var gameId = await GamesService.AddGameResultAsync(gameResult);
        var game = await GamesService.TryGetGameAsync(gameId);

        Assert.That(game, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(game!.Name, Is.EqualTo(gameResult.Name));
            Assert.That(game.LanguageCode, Is.EqualTo(gameResult.LanguageCode));
            Assert.That(game.Package.Name, Is.EqualTo(gameResult.Package.Name));
        });
    }

    [Test]
    public async Task AddGameResultAsync_HandleDuplicates_Ok()
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult);
        Assert.DoesNotThrowAsync(() => GamesService.AddGameResultAsync(gameResult));
    }

    [Test]
    public async Task AddGameResultAsync_PackageWithAuthorContacts_UpdateContacts()
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        var gameId = await GamesService.AddGameResultAsync(gameResult);

        var game = await GamesService.TryGetGameAsync(gameId);
        Assert.That(game, Is.Not.Null);
        Assert.That(game!.Package.AuthorsContacts, Is.Null);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"], "Contact value"))
        {
            Name = $"Test game 2 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        Assert.DoesNotThrowAsync(() => GamesService.AddGameResultAsync(gameResult2));

        game = await GamesService.TryGetGameAsync(gameId);
        Assert.That(game, Is.Not.Null);
        Assert.That(game!.Package.AuthorsContacts, Is.EqualTo("Contact value"));

        var gameResult3 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        var gameId3 = await GamesService.AddGameResultAsync(gameResult3);

        var game3 = await GamesService.TryGetGameAsync(gameId3);
        Assert.That(game3, Is.Not.Null);
        Assert.That(game3!.Package.AuthorsContacts, Is.EqualTo("Contact value"));
    }

    [TestCase(1)]
    [TestCase(10)]
    public async Task GetGamesByFilter_CountFilter_Ok(int count)
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"], "Contact value"))
        {
            Name = $"Test game 2 {randomId}",
            Results = [],
            Reviews = [],
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

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now,
            LanguageCode = "en"
        };

        await GamesService.AddGameResultAsync(gameResult);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"], "Contact value"))
        {
            Name = $"Test game 2 {randomId}",
            Results = [],
            Reviews = [],
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

        var gameResult = new GameResultInfo(new PackageInfo($"Test package {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        await GamesService.AddGameResultAsync(gameResult);

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package 2 {randomId}", "", [$"Test author 1 {randomId}", $"Test author 2 {randomId}"], "Contact value"))
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

    #region Package Stats Tests

    [Test]
    public async Task AddGameResultAsync_WithPackageStats_StoresStatsCorrectly()
    {
        var randomId = Guid.NewGuid();
        var packageInfo = new PackageInfo($"Test package {randomId}", "hash123", [$"Test author {randomId}"]);

        var gameResult = new GameResultInfo(packageInfo)
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        var packageStats = new PackageStats(
            new PackageTopLevelStats(5),
            new Dictionary<string, QuestionStats>
            {
                ["question1"] = new QuestionStats(10, 8, 3, 5),
                ["question2"] = new QuestionStats(15, 12, 7, 5)
            }
        );

        // Add game with package stats
        await GamesService.AddGameResultAsync(gameResult, packageStats);

        // Retrieve and verify stats
        var request = new PackageStatsRequest(packageInfo.Name!, packageInfo.Hash, packageInfo.Authors);
        var retrievedStats = await GamesService.GetPackageStatsAsync(request);

        Assert.That(retrievedStats, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(retrievedStats!.TopLevelStats.CompletedGameCount, Is.EqualTo(5));
            Assert.That(retrievedStats.QuestionStats, Has.Count.EqualTo(2));

            Assert.That(retrievedStats.QuestionStats["question1"].ShownCount, Is.EqualTo(10));
            Assert.That(retrievedStats.QuestionStats["question1"].PlayerSeenCount, Is.EqualTo(8));
            Assert.That(retrievedStats.QuestionStats["question1"].CorrectCount, Is.EqualTo(3));
            Assert.That(retrievedStats.QuestionStats["question1"].WrongCount, Is.EqualTo(5));

            Assert.That(retrievedStats.QuestionStats["question2"].ShownCount, Is.EqualTo(15));
            Assert.That(retrievedStats.QuestionStats["question2"].PlayerSeenCount, Is.EqualTo(12));
            Assert.That(retrievedStats.QuestionStats["question2"].CorrectCount, Is.EqualTo(7));
            Assert.That(retrievedStats.QuestionStats["question2"].WrongCount, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task AddGameResultAsync_WithoutPackageStats_ReturnsNull()
    {
        var randomId = Guid.NewGuid();
        var packageInfo = new PackageInfo($"Test package {randomId}", "hash456", [$"Test author {randomId}"]);

        var gameResult = new GameResultInfo(packageInfo)
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        // Add game without package stats
        await GamesService.AddGameResultAsync(gameResult, null);

        // Try to retrieve stats
        var request = new PackageStatsRequest(packageInfo.Name!, packageInfo.Hash, packageInfo.Authors);
        var retrievedStats = await GamesService.GetPackageStatsAsync(request);

        Assert.That(retrievedStats, Is.Null);
    }

    [Test]
    public async Task AddGameResultAsync_MergeExistingPackageStats_MergesCorrectly()
    {
        var randomId = Guid.NewGuid();
        var packageInfo = new PackageInfo($"Test package {randomId}", "hash789", [$"Test author {randomId}"]);
        
        var gameResult1 = new GameResultInfo(packageInfo)
        {
            Name = $"Test game 1 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        var firstStats = new PackageStats(
            new PackageTopLevelStats(3),
            new Dictionary<string, QuestionStats>
            {
                ["question1"] = new QuestionStats(5, 4, 2, 1),
                ["question2"] = new QuestionStats(8, 6, 3, 2)
            }
        );

        // Add first game with stats
        await GamesService.AddGameResultAsync(gameResult1, firstStats);

        var gameResult2 = new GameResultInfo(packageInfo)
        {
            Name = $"Test game 2 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        var secondStats = new PackageStats(
            new PackageTopLevelStats(2),
            new Dictionary<string, QuestionStats>
            {
                ["question1"] = new QuestionStats(3, 2, 1, 2), // Should be merged with existing
                ["question3"] = new QuestionStats(4, 3, 2, 1)  // New question
            }
        );

        // Add second game with stats
        await GamesService.AddGameResultAsync(gameResult2, secondStats);

        // Retrieve and verify merged stats
        var request = new PackageStatsRequest(packageInfo.Name!, packageInfo.Hash, packageInfo.Authors);
        var mergedStats = await GamesService.GetPackageStatsAsync(request);

        Assert.That(mergedStats, Is.Not.Null);
        Assert.Multiple(() =>
        {
            // Top-level stats should be summed
            Assert.That(mergedStats!.TopLevelStats.CompletedGameCount, Is.EqualTo(5)); // 3 + 2

            Assert.That(mergedStats.QuestionStats, Has.Count.EqualTo(3));

            // question1 should be merged (5+3, 4+2, 2+1, 1+2)
            Assert.That(mergedStats.QuestionStats["question1"].ShownCount, Is.EqualTo(8));
            Assert.That(mergedStats.QuestionStats["question1"].PlayerSeenCount, Is.EqualTo(6));
            Assert.That(mergedStats.QuestionStats["question1"].CorrectCount, Is.EqualTo(3));
            Assert.That(mergedStats.QuestionStats["question1"].WrongCount, Is.EqualTo(3));

            // question2 should remain unchanged (only in first stats)
            Assert.That(mergedStats.QuestionStats["question2"].ShownCount, Is.EqualTo(8));
            Assert.That(mergedStats.QuestionStats["question2"].PlayerSeenCount, Is.EqualTo(6));
            Assert.That(mergedStats.QuestionStats["question2"].CorrectCount, Is.EqualTo(3));
            Assert.That(mergedStats.QuestionStats["question2"].WrongCount, Is.EqualTo(2));

            // question3 should be new (only in second stats)
            Assert.That(mergedStats.QuestionStats["question3"].ShownCount, Is.EqualTo(4));
            Assert.That(mergedStats.QuestionStats["question3"].PlayerSeenCount, Is.EqualTo(3));
            Assert.That(mergedStats.QuestionStats["question3"].CorrectCount, Is.EqualTo(2));
            Assert.That(mergedStats.QuestionStats["question3"].WrongCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetPackageStatsAsync_NonExistentPackage_ReturnsNull()
    {
        var request = new PackageStatsRequest("Non-existent Package", "nonhash", ["Non-existent Author"]);
        var stats = await GamesService.GetPackageStatsAsync(request);

        Assert.That(stats, Is.Null);
    }

    [Test]
    public async Task AddGameResultAsync_EmptyPackageStats_StoresEmptyStats()
    {
        var randomId = Guid.NewGuid();
        var packageInfo = new PackageInfo($"Test package {randomId}", "emptyhash", [$"Test author {randomId}"]);

        var gameResult = new GameResultInfo(packageInfo)
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = DateTimeOffset.Now
        };

        var emptyStats = new PackageStats(
            new PackageTopLevelStats(1),
            []
        );

        // Add game with empty question stats
        await GamesService.AddGameResultAsync(gameResult, emptyStats);

        // Retrieve and verify stats
        var request = new PackageStatsRequest(packageInfo.Name!, packageInfo.Hash, packageInfo.Authors);
        var retrievedStats = await GamesService.GetPackageStatsAsync(request);

        Assert.That(retrievedStats, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(retrievedStats!.TopLevelStats.CompletedGameCount, Is.EqualTo(1));
            Assert.That(retrievedStats.QuestionStats, Is.Empty);
        });
    }

    [Test]
    public async Task AddGameResultAsync_MultipleGamesWithStats_AccumulatesStatsCorrectly()
    {
        var randomId = Guid.NewGuid();
        var packageInfo = new PackageInfo($"Multi Test package {randomId}", "multihash", [$"Multi author {randomId}"]);

        // Add three games with incrementing stats
        for (int i = 1; i <= 3; i++)
        {
            var gameResult = new GameResultInfo(packageInfo)
            {
                Name = $"Multi game {i} {randomId}",
                Results = [],
                Reviews = [],
                FinishTime = DateTimeOffset.Now
            };

            var stats = new PackageStats(
                new PackageTopLevelStats(i), // 1, 2, 3
                new Dictionary<string, QuestionStats>
                {
                    ["common_question"] = new QuestionStats(i, i, i, i) // Incrementing values
                }
            );

            await GamesService.AddGameResultAsync(gameResult, stats);
        }

        // Retrieve and verify accumulated stats
        var request = new PackageStatsRequest(packageInfo.Name!, packageInfo.Hash, packageInfo.Authors);
        var finalStats = await GamesService.GetPackageStatsAsync(request);

        Assert.That(finalStats, Is.Not.Null);
        Assert.Multiple(() =>
        {
            // Should be 1 + 2 + 3 = 6
            Assert.That(finalStats!.TopLevelStats.CompletedGameCount, Is.EqualTo(6));

            // Should be sum of all iterations (1+2+3 = 6 for each field)
            Assert.That(finalStats.QuestionStats["common_question"].ShownCount, Is.EqualTo(6));
            Assert.That(finalStats.QuestionStats["common_question"].PlayerSeenCount, Is.EqualTo(6));
            Assert.That(finalStats.QuestionStats["common_question"].CorrectCount, Is.EqualTo(6));
            Assert.That(finalStats.QuestionStats["common_question"].WrongCount, Is.EqualTo(6));
        });
    }

    [Test]
    public async Task GetPackageStatsAsync_PackageIdentification_MatchesExactly()
    {
        var randomId = Guid.NewGuid();

        // Create two similar but different packages
        var package1Info = new PackageInfo($"Package {randomId}", "hash1", [$"Author A {randomId}"]);
        var package2Info = new PackageInfo($"Package {randomId}", "hash2", [$"Author A {randomId}"]); // Different hash
        var package3Info = new PackageInfo($"Package {randomId}", "hash1", [$"Author B {randomId}"]); // Different author

        var stats1 = new PackageStats(
            new PackageTopLevelStats(10),
            new Dictionary<string, QuestionStats> { ["q1"] = new QuestionStats(1, 1, 1, 1) }
        );

        var stats2 = new PackageStats(
            new PackageTopLevelStats(20),
            new Dictionary<string, QuestionStats> { ["q2"] = new QuestionStats(2, 2, 2, 2) }
        );

        var stats3 = new PackageStats(
            new PackageTopLevelStats(30),
            new Dictionary<string, QuestionStats> { ["q3"] = new QuestionStats(3, 3, 3, 3) }
        );

        // Add games for each package
        await GamesService.AddGameResultAsync(
            new GameResultInfo(package1Info) { Name = "Game1", FinishTime = DateTimeOffset.Now, Results = [], Reviews = [] },
            stats1);

        await GamesService.AddGameResultAsync(
            new GameResultInfo(package2Info) { Name = "Game2", FinishTime = DateTimeOffset.Now, Results = [], Reviews = [] },
            stats2);

        await GamesService.AddGameResultAsync(
            new GameResultInfo(package3Info) { Name = "Game3", FinishTime = DateTimeOffset.Now, Results = [], Reviews = [] },
            stats3);

        // Verify each package has its own distinct stats
        var retrievedStats1 = await GamesService.GetPackageStatsAsync(
            new PackageStatsRequest(package1Info.Name!, package1Info.Hash, package1Info.Authors));
        var retrievedStats2 = await GamesService.GetPackageStatsAsync(
            new PackageStatsRequest(package2Info.Name!, package2Info.Hash, package2Info.Authors));
        var retrievedStats3 = await GamesService.GetPackageStatsAsync(
            new PackageStatsRequest(package3Info.Name!, package3Info.Hash, package3Info.Authors));

        Assert.Multiple(() =>
        {
            Assert.That(retrievedStats1?.TopLevelStats.CompletedGameCount, Is.EqualTo(10));
            Assert.That(retrievedStats2?.TopLevelStats.CompletedGameCount, Is.EqualTo(20));
            Assert.That(retrievedStats3?.TopLevelStats.CompletedGameCount, Is.EqualTo(30));

            Assert.That(retrievedStats1?.QuestionStats.Keys, Is.EquivalentTo(["q1"]));
            Assert.That(retrievedStats2?.QuestionStats.Keys, Is.EquivalentTo(["q2"]));
            Assert.That(retrievedStats3?.QuestionStats.Keys, Is.EquivalentTo(["q3"]));
        });
    }

    #endregion
}
