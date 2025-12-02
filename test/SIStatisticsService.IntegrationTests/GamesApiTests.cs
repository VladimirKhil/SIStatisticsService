using SIStatisticsService.Client;
using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.IntegrationTests;

internal sealed class GamesApiTests : TestsBase
{
    [Test]
    public void SendLocalGameStatistic_InvalidFinishTime_Fail()
    {
        var exception = Assert.ThrowsAsync<SIStatisticsClientException>(() => SIStatisticsClient.SendGameReportAsync(new GameReport
        {
            Info = new GameResultInfo(new PackageInfo("TestPackage", "", ["TestAuthor"]))
            {
                FinishTime = new DateTimeOffset(2023, 3, 18, 12, 0, 0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(40),
                Name = "TestGame",
                Platform = GamePlatforms.Local,
                Results = new Dictionary<string, int>
                {
                    ["Alice"] = 1999,
                    ["Bob"] = 2,
                    ["Clara"] = -1700
                },
                Reviews = new Dictionary<string, string>
                {
                    ["Alice"] = "A lot of fun"
                }
            }
        }));

        Assert.That(exception!.ErrorCode, Is.EqualTo(WellKnownSIStatisticServiceErrorCode.InvalidFinishTime));
    }

    [Test]
    public async Task SendLocalGameStatistic_Ok()
    {
        var uniqueName = "_TEST_" + Guid.NewGuid().ToString();

        var gameResultInfo = new GameResultInfo(new PackageInfo("TestPackage", "1", new[] { "TestAuthor" }))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(40),
            Name = uniqueName,
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                ["Alice"] = 1999,
                ["Bob"] = 2,
                ["Clara"] = -1700
            },
            Reviews = new Dictionary<string, string>
            {
                ["Alice"] = "A lot of fun"
            }
        };

        var gameReport = new GameReport
        {
            Info = gameResultInfo
        };

        await SIStatisticsClient.SendGameReportAsync(gameReport);

        for (int i = 0; i < 8; i++)
        {
            await AddPackageGamesAsync("TestPackage");
        }

        var gameResultInfo2 = new GameResultInfo(new PackageInfo("TestPackage 2", "2", new[] { "TestAuthor 2" }))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(10),
            Name = uniqueName + "_2",
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                ["Bett"] = 10_000,
                ["Simon"] = 50,
                ["Ken"] = 3555,
                ["George"] = 800
            },
            Reviews = []
        };

        var gameReport2 = new GameReport
        {
            Info = gameResultInfo2
        };

        await SIStatisticsClient.SendGameReportAsync(gameReport2);

        for (int i = 0; i < 4; i++)
        {
            await AddPackageGamesAsync("TestPackage 2");
        }

        var gameResultInfo3 = new GameResultInfo(new PackageInfo("TestPackage", "1", new[] { "TestAuthor" }))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromHours(1),
            Name = uniqueName + "_3",
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                ["Nora"] = 3,
                ["Peter"] = 2000
            },
            Reviews = new Dictionary<string, string>
            {
                ["Peter"] = "I like it",
                ["Nora"] = "gg"
            }
        };

        var gameReport3 = new GameReport
        {
            Info = gameResultInfo3
        };

        await SIStatisticsClient.SendGameReportAsync(gameReport3);

        var gameResultInfo4 = new GameResultInfo(new PackageInfo("TestPackage", "1", new[] { "TestAuthor" }))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromHours(2),
            Name = uniqueName + "_4",
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                ["Ted"] = 3000,
                ["John"] = -20000
            },
            Reviews = []
        };

        var gameReport4 = new GameReport
        {
            Info = gameResultInfo4
        };

        await SIStatisticsClient.SendGameReportAsync(gameReport4);

        var gamesInfo = await SIStatisticsClient.GetLatestGamesInfoAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)),
                To = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1)),
                Platform = GamePlatforms.Local,
            });

        Assert.That(gamesInfo, Is.Not.Null);
        Assert.That(gamesInfo!.Results, Has.Length.GreaterThanOrEqualTo(1), "No results found");

        var gameResult = gamesInfo.Results.FirstOrDefault(r => r.Name == uniqueName);
        Assert.That(gameResult, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(gameResult!.FinishTime, Is.EqualTo(gameResultInfo.FinishTime).Within(TimeSpan.FromMilliseconds(100)));
            Assert.That(gameResult.Duration, Is.EqualTo(gameResultInfo.Duration));
            Assert.That(gameResult.Name, Is.EqualTo(gameResultInfo.Name));
            Assert.That(gameResult.Package.Name, Is.EqualTo(gameResultInfo.Package.Name));
            Assert.That(gameResultInfo.Package.Authors, Is.EquivalentTo(gameResult.Package.Authors));
            Assert.That(gameResult.Platform, Is.EqualTo(gameResultInfo.Platform));
            Assert.That(gameResult.Results.Count, Is.EqualTo(gameResultInfo.Results.Count));
            Assert.That(gameResultInfo.Results.Keys, Is.EquivalentTo(gameResult.Results.Keys));
            Assert.That(gameResultInfo.Results.Values, Is.EquivalentTo(gameResult.Results.Values));
            Assert.That(gameResult.Reviews, Is.EqualTo(gameResultInfo.Reviews));
            Assert.That(gameResultInfo.Reviews.Keys, Is.EquivalentTo(gameResult.Reviews.Keys));
            Assert.That(gameResultInfo.Reviews.Values, Is.EquivalentTo(gameResult.Reviews.Values));
        });

        var statistic = await SIStatisticsClient.GetLatestGamesStatisticAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)),
                To = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1)),
                Platform = GamePlatforms.Local,
            });

        Assert.That(statistic, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(statistic!.GameCount, Is.GreaterThan(2));
            Assert.That(statistic.TotalDuration, Is.GreaterThanOrEqualTo(TimeSpan.FromMinutes(40)));
        });

        var packages = await SIStatisticsClient.GetLatestTopPackagesAsync(
            new TopPackagesRequest(new StatisticFilter
            {
                From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)),
                To = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1)),
                Platform = GamePlatforms.Local,
            }));

        Assert.That(packages, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(packages!.Packages, Has.Length.GreaterThan(1));

            var testPackage = packages.Packages.FirstOrDefault(p => p.Package?.Name == "TestPackage");
            var testPackage2 = packages.Packages.FirstOrDefault(p => p.Package?.Name == "TestPackage 2");

            Assert.That(testPackage, Is.Not.Null);
            Assert.That(testPackage2, Is.Not.Null);
            Assert.That(testPackage!.GameCount, Is.GreaterThan(testPackage2!.GameCount));
        });
    }

    [Test]
    public async Task SendGameReportWithPackageStats_StoreAndRetrieve_Ok()
    {
        // Arrange - Create unique identifiers for this test
        var testId = Guid.NewGuid().ToString();
        var packageName = $"StatsTestPackage_{testId}";
        var packageHash = $"stats_hash_{testId}";
        var packageAuthor = $"StatsTestAuthor_{testId}";

        var gameResultInfo = new GameResultInfo(new PackageInfo(packageName, packageHash, [packageAuthor]))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(45),
            Name = $"StatsTestGame_{testId}",
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                ["Player1"] = 1500,
                ["Player2"] = 1200,
                ["Player3"] = 800
            },
            Reviews = new Dictionary<string, string>
            {
                ["Player1"] = "Excellent game with great questions!",
                ["Player3"] = "Good package, enjoyed it"
            }
        };

        // Create comprehensive package stats to test
        var packageStats = new PackageStats(
            new PackageTopLevelStats(15, 15), // 15 completed games
            new Dictionary<string, QuestionStats>
            {
                ["Round1_Science_Q1"] = new QuestionStats(50, 45, 30, 32, 13), // High success rate question
                ["Round1_Science_Q2"] = new QuestionStats(48, 42, 40, 28, 14),
                ["Round1_History_Q1"] = new QuestionStats(55, 50, 50, 35, 15),
                ["Round2_Literature_Q1"] = new QuestionStats(40, 38, 37, 25, 13),
                ["Round2_Geography_Q1"] = new QuestionStats(45, 40, 40, 20, 20), // Lower success rate
                ["Final_Mixed_Q1"] = new QuestionStats(30, 28, 20, 18, 10) // Final round question
            }
        );

        var gameReport = new GameReport
        {
            Id = Guid.NewGuid(),
            Info = gameResultInfo,
            Stats = packageStats,
            QuestionReports = []
        };

        // Act 1 - Send game report with stats
        await SIStatisticsClient.SendGameReportAsync(gameReport);

        // Act 2 - Retrieve and verify the game was stored
        var gamesInfo = await SIStatisticsClient.GetLatestGamesInfoAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5)),
                To = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(5)),
                Platform = GamePlatforms.Local,
            });

        Assert.That(gamesInfo, Is.Not.Null);
        var storedGame = gamesInfo!.Results.FirstOrDefault(r => r.Name == gameResultInfo.Name);
        Assert.That(storedGame, Is.Not.Null, "Game should have been stored");

        // Verify game data integrity
        Assert.Multiple(() =>
        {
            Assert.That(storedGame!.Package.Name, Is.EqualTo(packageName));
            Assert.That(storedGame.Package.Hash, Is.EqualTo(packageHash));
            Assert.That(storedGame.Package.Authors, Is.EquivalentTo(new[] { packageAuthor }));
            Assert.That(storedGame.Results.Count, Is.EqualTo(3));
            Assert.That(storedGame.Reviews.Count, Is.EqualTo(2));
            Assert.That(storedGame.Duration, Is.EqualTo(TimeSpan.FromMinutes(45)));
        });

        // Act 3 - Retrieve and verify package stats were stored
        var packageStatsRequest = new PackageStatsRequest(packageName, packageHash, [packageAuthor]);
        var retrievedStats = await SIStatisticsClient.GetPackageStats(packageStatsRequest);

        // Assert - Verify stats were properly stored and can be retrieved
        Assert.That(retrievedStats, Is.Not.Null, "Package stats should have been stored and retrievable");

        Assert.Multiple(() =>
        {
            // Verify top-level stats
            Assert.That(retrievedStats!.TopLevelStats.CompletedGameCount, Is.EqualTo(15));
            
            // Verify question-level stats
            Assert.That(retrievedStats.QuestionStats, Has.Count.EqualTo(6));
            
            // Check specific question stats to ensure data integrity
            var scienceQ1 = retrievedStats.QuestionStats["Round1_Science_Q1"];
            Assert.That(scienceQ1.ShownCount, Is.EqualTo(50));
            Assert.That(scienceQ1.PlayerSeenCount, Is.EqualTo(45));
            Assert.That(scienceQ1.CorrectCount, Is.EqualTo(32));
            Assert.That(scienceQ1.WrongCount, Is.EqualTo(13));

            var historyQ1 = retrievedStats.QuestionStats["Round1_History_Q1"];
            Assert.That(historyQ1.ShownCount, Is.EqualTo(55));
            Assert.That(historyQ1.PlayerSeenCount, Is.EqualTo(50));
            Assert.That(historyQ1.CorrectCount, Is.EqualTo(35));
            Assert.That(historyQ1.WrongCount, Is.EqualTo(15));

            var geoQ1 = retrievedStats.QuestionStats["Round2_Geography_Q1"];
            Assert.That(geoQ1.ShownCount, Is.EqualTo(45));
            Assert.That(geoQ1.PlayerSeenCount, Is.EqualTo(40));
            Assert.That(geoQ1.CorrectCount, Is.EqualTo(20));
            Assert.That(geoQ1.WrongCount, Is.EqualTo(20));

            var finalQ1 = retrievedStats.QuestionStats["Final_Mixed_Q1"];
            Assert.That(finalQ1.ShownCount, Is.EqualTo(30));
            Assert.That(finalQ1.PlayerSeenCount, Is.EqualTo(28));
            Assert.That(finalQ1.CorrectCount, Is.EqualTo(18));
            Assert.That(finalQ1.WrongCount, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task PackageStatsAccumulation_MultipleSubmissions_MergeCorrectly()
    {
        // Test that demonstrates package stats accumulation over multiple game submissions

        var testId = Guid.NewGuid().ToString();
        var packageName = $"AccumTestPackage_{testId}";
        var packageHash = $"accum_hash_{testId}";
        var packageAuthor = $"AccumTestAuthor_{testId}";

        // First submission with initial stats
        var firstGameInfo = new GameResultInfo(new PackageInfo(packageName, packageHash, [packageAuthor]))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(30),
            Name = $"AccumGame1_{testId}",
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int> { ["Player1"] = 1000 },
            Reviews = new Dictionary<string, string> { ["Player1"] = "First game review" }
        };

        var firstStats = new PackageStats(
            new PackageTopLevelStats(5, 5), // 5 completed games in first batch
            new Dictionary<string, QuestionStats>
            {
                ["common_question"] = new QuestionStats(20, 18, 10, 12, 6),
                ["first_batch_question"] = new QuestionStats(15, 14, 7, 10, 4)
            }
        );

        var firstReport = new GameReport
        {
            Id = Guid.NewGuid(),
            Info = firstGameInfo,
            Stats = firstStats,
            QuestionReports = Array.Empty<QuestionReport>()
        };

        // Second submission with additional stats that should merge
        var secondGameInfo = new GameResultInfo(new PackageInfo(packageName, packageHash, [packageAuthor]))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(35),
            Name = $"AccumGame2_{testId}",
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int> { ["Player2"] = 1200 },
            Reviews = new Dictionary<string, string> { ["Player2"] = "Second game review" }
        };

        var secondStats = new PackageStats(
            new PackageTopLevelStats(3, 3), // 3 additional completed games
            new Dictionary<string, QuestionStats>
            {
                ["common_question"] = new QuestionStats(12, 10, 6, 8, 2), // Should merge with existing
                ["second_batch_question"] = new QuestionStats(8, 7, 5, 5, 2) // New question
            }
        );

        var secondReport = new GameReport
        {
            Id = Guid.NewGuid(),
            Info = secondGameInfo,
            Stats = secondStats,
            QuestionReports = Array.Empty<QuestionReport>()
        };

        // Submit both reports
        await SIStatisticsClient.SendGameReportAsync(firstReport);
        await SIStatisticsClient.SendGameReportAsync(secondReport);

        // Verify both games were stored
        var gamesInfo = await SIStatisticsClient.GetLatestGamesInfoAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5)),
                To = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(5)),
                Platform = GamePlatforms.Local,
            });

        Assert.That(gamesInfo, Is.Not.Null);

        var firstGame = gamesInfo!.Results.FirstOrDefault(r => r.Name == firstGameInfo.Name);
        var secondGame = gamesInfo.Results.FirstOrDefault(r => r.Name == secondGameInfo.Name);

        Assert.Multiple(() =>
        {
            Assert.That(firstGame, Is.Not.Null, "First game should have been stored");
            Assert.That(secondGame, Is.Not.Null, "Second game should have been stored");

            // Verify both games reference the same package
            Assert.That(firstGame!.Package.Name, Is.EqualTo(packageName));
            Assert.That(secondGame!.Package.Name, Is.EqualTo(packageName));
            Assert.That(firstGame.Package.Hash, Is.EqualTo(packageHash));
            Assert.That(secondGame.Package.Hash, Is.EqualTo(packageHash));
        });

        // Retrieve merged package stats
        var packageStatsRequest = new PackageStatsRequest(packageName, packageHash, [packageAuthor]);
        var mergedStats = await SIStatisticsClient.GetPackageStats(packageStatsRequest);

        // Verify stats were merged correctly
        Assert.That(mergedStats, Is.Not.Null, "Merged package stats should be available");

        Assert.Multiple(() =>
        {
            // Top-level stats should be summed: 5 + 3 = 8
            Assert.That(mergedStats!.TopLevelStats.CompletedGameCount, Is.EqualTo(8));

            // Should have all questions from both submissions
            Assert.That(mergedStats.QuestionStats, Has.Count.EqualTo(3));
            Assert.That(mergedStats.QuestionStats.Keys,
                Is.EquivalentTo(new[] { "common_question", "first_batch_question", "second_batch_question" }));

            // Common question should be merged: (20+12, 18+10, 12+8, 6+2)
            var commonStats = mergedStats.QuestionStats["common_question"];
            Assert.That(commonStats.ShownCount, Is.EqualTo(32));
            Assert.That(commonStats.PlayerSeenCount, Is.EqualTo(28));
            Assert.That(commonStats.CorrectCount, Is.EqualTo(20));
            Assert.That(commonStats.WrongCount, Is.EqualTo(8));

            // First batch question should remain unchanged
            var firstBatchStats = mergedStats.QuestionStats["first_batch_question"];
            Assert.That(firstBatchStats.ShownCount, Is.EqualTo(15));
            Assert.That(firstBatchStats.PlayerSeenCount, Is.EqualTo(14));
            Assert.That(firstBatchStats.CorrectCount, Is.EqualTo(10));
            Assert.That(firstBatchStats.WrongCount, Is.EqualTo(4));

            // Second batch question should remain unchanged
            var secondBatchStats = mergedStats.QuestionStats["second_batch_question"];
            Assert.That(secondBatchStats.ShownCount, Is.EqualTo(8));
            Assert.That(secondBatchStats.PlayerSeenCount, Is.EqualTo(7));
            Assert.That(secondBatchStats.CorrectCount, Is.EqualTo(5));
            Assert.That(secondBatchStats.WrongCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task PackageStatsWithUnicodeData_StoreAndRetrieve_Ok()
    {
        // Test package stats with international characters and complex data

        var testId = Guid.NewGuid().ToString();
        var packageName = $"Пакет_测试_{testId}"; // Russian and Chinese characters
        var packageHash = $"unicode_hash_🎮_{testId}"; // Emoji in hash
        var packageAuthor = $"Автор_作者_{testId}"; // Mixed languages

        var gameResultInfo = new GameResultInfo(new PackageInfo(packageName, packageHash, [packageAuthor]))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(50),
            Name = $"Игра_ゲーム_{testId}", // Game name with international chars
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                ["Игрок_1"] = 1800, // Russian player name
                ["プレーヤー_2"] = 1600, // Japanese player name
                ["Joueur_3"] = 1400 // French player name
            },
            Reviews = new Dictionary<string, string>
            {
                ["Игрок_1"] = "Отличная игра! 素晴らしい！", // Mixed language review
                ["プレーヤー_2"] = "とても面白いゲームでした。Great game!"
            }
        };

        var packageStats = new PackageStats(
            new PackageTopLevelStats(25, 25),
            new Dictionary<string, QuestionStats>
            {
                // Question keys with various international characters and symbols
                ["Тема: История России_Вопрос №1"] = new QuestionStats(100, 85, 70, 70, 15), // Cyrillic
                ["テーマ：日本の文化_質問1"] = new QuestionStats(80, 70, 55, 55, 15), // Japanese
                ["Thème: Culture française_Question #1"] = new QuestionStats(90, 75, 60, 60, 15), // French
                ["主题：中国历史_问题1"] = new QuestionStats(85, 72, 60, 58, 14), // Chinese
                ["Special@Chars_Question!@#$%^&*()"] = new QuestionStats(60, 50, 40, 35, 15) // Special chars
            }
        );

        var gameReport = new GameReport
        {
            Id = Guid.NewGuid(),
            Info = gameResultInfo,
            Stats = packageStats,
            QuestionReports = Array.Empty<QuestionReport>()
        };

        // Submit the game report
        await SIStatisticsClient.SendGameReportAsync(gameReport);

        // Verify game was stored with correct international data
        var gamesInfo = await SIStatisticsClient.GetLatestGamesInfoAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5)),
                To = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(5)),
                Platform = GamePlatforms.Local,
            });

        Assert.That(gamesInfo, Is.Not.Null);
        var storedGame = gamesInfo!.Results.FirstOrDefault(r => r.Name == gameResultInfo.Name);
        Assert.That(storedGame, Is.Not.Null, "Unicode game should have been stored");

        // Retrieve and verify package stats with unicode data
        var packageStatsRequest = new PackageStatsRequest(packageName, packageHash, [packageAuthor]);
        var retrievedStats = await SIStatisticsClient.GetPackageStats(packageStatsRequest);

        Assert.That(retrievedStats, Is.Not.Null, "Unicode package stats should be retrievable");

        Assert.Multiple(() =>
        {
            Assert.That(retrievedStats!.TopLevelStats.CompletedGameCount, Is.EqualTo(25));
            Assert.That(retrievedStats.QuestionStats, Has.Count.EqualTo(5));

            // Verify all unicode question keys are preserved
            var expectedKeys = new[]
            {
                "Тема: История России_Вопрос №1",
                "テーマ：日本の文化_質問1",
                "Thème: Culture française_Question #1",
                "主题：中国历史_问题1",
                "Special@Chars_Question!@#$%^&*()"
            };

            Assert.That(retrievedStats.QuestionStats.Keys, Is.EquivalentTo(expectedKeys));

            // Verify specific stats for each international question
            var russianStats = retrievedStats.QuestionStats["Тема: История России_Вопрос №1"];
            Assert.That(russianStats.ShownCount, Is.EqualTo(100));
            Assert.That(russianStats.PlayerSeenCount, Is.EqualTo(85));
            Assert.That(russianStats.CorrectCount, Is.EqualTo(70));
            Assert.That(russianStats.WrongCount, Is.EqualTo(15));

            var japaneseStats = retrievedStats.QuestionStats["テーマ：日本の文化_質問1"];
            Assert.That(japaneseStats.ShownCount, Is.EqualTo(80));
            Assert.That(japaneseStats.PlayerSeenCount, Is.EqualTo(70));
            Assert.That(japaneseStats.CorrectCount, Is.EqualTo(55));
            Assert.That(japaneseStats.WrongCount, Is.EqualTo(15));

            var chineseStats = retrievedStats.QuestionStats["主题：中国历史_问题1"];
            Assert.That(chineseStats.ShownCount, Is.EqualTo(85));
            Assert.That(chineseStats.PlayerSeenCount, Is.EqualTo(72));
            Assert.That(chineseStats.CorrectCount, Is.EqualTo(58));
            Assert.That(chineseStats.WrongCount, Is.EqualTo(14));

            var specialCharsStats = retrievedStats.QuestionStats["Special@Chars_Question!@#$%^&*()"];
            Assert.That(specialCharsStats.ShownCount, Is.EqualTo(60));
            Assert.That(specialCharsStats.PlayerSeenCount, Is.EqualTo(50));
            Assert.That(specialCharsStats.CorrectCount, Is.EqualTo(35));
            Assert.That(specialCharsStats.WrongCount, Is.EqualTo(15));
        });
    }

    private Task AddPackageGamesAsync(string packageName, string? languageCode = null)
    {
        var gameResultInfo2 = new GameResultInfo(new PackageInfo(packageName, "2", ["TestAuthor 2"]), languageCode)
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(10),
            Name = "_TEST_" + Guid.NewGuid().ToString(),
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                { "Player", 2000 }
            },
            Reviews = []
        };

        var gameReport2 = new GameReport
        {
            Info = gameResultInfo2
        };

        return SIStatisticsClient.SendGameReportAsync(gameReport2);
    }

    [Test]
    public async Task SendLocalGame_QuestionReports_Ok()
    {
        var uniqueName = "_TEST_" + Guid.NewGuid().ToString();

        var gameResultInfo = new GameResultInfo(new PackageInfo("TestPackage", "1", new[] { "TestAuthor" }))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(40),
            Name = uniqueName,
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>
            {
                ["Alice"] = 1999,
                ["Bob"] = 2,
                ["Clara"] = -1700
            },
            Reviews = new Dictionary<string, string>
            {
                ["Alice"] = "A lot of fun"
            }
        };

        var gameReport = new GameReport
        {
            Info = gameResultInfo,
            QuestionReports =
            [
                new()
                {
                    ThemeName = "Test theme",
                    QuestionText = "Test question",
                    ReportText = "Test accept",
                    ReportType = QuestionReportType.Accepted
                },
                new()
                {
                    ThemeName = "Test theme",
                    QuestionText = "Test question",
                    ReportText = "Test reject",
                    ReportType = QuestionReportType.Rejected
                },
                new()
                {
                    ThemeName = "Test theme",
                    QuestionText = "Test question",
                    ReportText = "Test apellate",
                    ReportType = QuestionReportType.Apellated
                },
                new()
                {
                    ThemeName = "Test theme",
                    QuestionText = "Test question",
                    ReportText = "Test complain",
                    ReportType = QuestionReportType.Complained
                },
            ]
        };

        await SIStatisticsClient.SendGameReportAsync(gameReport);

        var questionInfo = await SIStatisticsClient.GetQuestionInfoAsync("Test theme", "Test question");

        Assert.That(questionInfo, Is.Not.Null);
        Assert.That(questionInfo!.Entities.Count, Is.GreaterThanOrEqualTo(4));

        var accepted = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test accept");
        Assert.That(accepted, Is.Not.Null);

        Assert.That(accepted, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(accepted!.RelationType, Is.EqualTo(RelationType.Accepted));

        var rejected = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test reject");
        Assert.That(rejected, Is.Not.Null);

        Assert.That(rejected, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(rejected!.RelationType, Is.EqualTo(RelationType.Rejected));

        var appellated = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test apellate");
        Assert.That(appellated, Is.Not.Null);

        Assert.That(appellated, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(appellated!.RelationType, Is.EqualTo(RelationType.Apellated));

        var complained = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test complain");
        Assert.That(complained, Is.Not.Null);

        Assert.That(complained, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(complained!.RelationType, Is.EqualTo(RelationType.Complained));
    }

    [Test]
    public async Task GetLatestGame_Ok()
    {
        await AddPackageGamesAsync("GetLatestGame_Ok");

        var statistics = await SIStatisticsClient.GetLatestGamesInfoAsync(new StatisticFilter
        {
            From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)),
            To = DateTimeOffset.UtcNow,
            Platform = GamePlatforms.Local,
            Count = 1
        });

        Assert.That(statistics, Is.Not.Null);
        Assert.That(statistics!.Results, Has.Length.EqualTo(1));
    }

    [TestCase("ru")]
    [TestCase("en")]
    public async Task GetLatestGame_LanguageCode_Ok(string languageCode)
    {
        await AddPackageGamesAsync("GetLatestGame_LanguageCode_Ok", languageCode);

        var statistics = await SIStatisticsClient.GetLatestGamesInfoAsync(new StatisticFilter
        {
            From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
            To = DateTimeOffset.UtcNow,
            Platform = GamePlatforms.Local,
            Count = 1,
            LanguageCode = languageCode
        });

        Assert.That(statistics, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(statistics!.Results, Has.Length.EqualTo(1));
            Assert.That(statistics.Results[0].LanguageCode, Is.EqualTo(languageCode));
        });
    }

    [Test]
    public void SendGameReport_EmptyInfo_Fail()
    {
        var exception = Assert.ThrowsAsync<SIStatisticsClientException>(() => SIStatisticsClient.SendGameReportAsync(new GameReport()));
        Assert.That(exception!.ErrorCode, Is.EqualTo(WellKnownSIStatisticServiceErrorCode.GameInfoNotFound));
    }

    [Test]
    public void SendGameReport_NoPlatform_Fail()
    {
        var exception = Assert.ThrowsAsync<SIStatisticsClientException>(() =>
            SIStatisticsClient.SendGameReportAsync(new GameReport
            {
                Info = new GameResultInfo(new PackageInfo("", "", []))
                {
                    FinishTime = DateTimeOffset.UtcNow,
                },
            }));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(exception.ErrorCode, Is.EqualTo(WellKnownSIStatisticServiceErrorCode.UnsupportedPlatform));
        });
    }

    [Test]
    public void SendGameReport_GameServerPlatform_Fail()
    {
        var exception = Assert.ThrowsAsync<SIStatisticsClientException>(() =>
            SIStatisticsClient.SendGameReportAsync(new GameReport
            {
                Info = new GameResultInfo(new PackageInfo("", "", []))
                {
                    Platform = GamePlatforms.GameServer,
                    FinishTime = DateTimeOffset.UtcNow,
                }
            }));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(exception.ErrorCode, Is.EqualTo(WellKnownSIStatisticServiceErrorCode.UnsupportedPlatform));
        });
    }

    [Test]
    public void SendGameReport_NoFinishTime_Fail()
    {
        var exception = Assert.ThrowsAsync<SIStatisticsClientException>(() =>
            SIStatisticsClient.SendGameReportAsync(new GameReport
            {
                Info = new GameResultInfo(new PackageInfo("", "", []))
                {
                    Platform = GamePlatforms.Local,
                }
            }));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(exception.ErrorCode, Is.EqualTo(WellKnownSIStatisticServiceErrorCode.InvalidFinishTime));
        });
    }

    [Test]
    public void SendGameReport_NullHash_Fail()
    {
        var exception = Assert.ThrowsAsync<SIStatisticsClientException>(() =>
            SIStatisticsClient.SendGameReportAsync(new GameReport
            {
                Info = new GameResultInfo(new PackageInfo("", null!, []))
                {
                    Platform = GamePlatforms.Local,
                    FinishTime = DateTimeOffset.UtcNow,
                }
            }));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(exception.ErrorCode, Is.EqualTo(WellKnownSIStatisticServiceErrorCode.MissingPackageHash));
        });
    }
}