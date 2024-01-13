using NUnit.Framework.Legacy;
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
            Info = new GameResultInfo(new PackageInfo("TestPackage", null, new[] { "TestAuthor" }))
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

        Assert.That(exception.ErrorCode, Is.EqualTo(WellKnownSIStatisticServiceErrorCode.InvalidFinishTime));
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
            Reviews = new Dictionary<string, string> { }
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
            Reviews = new Dictionary<string, string> { }
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
        Assert.That(gamesInfo.Results, Has.Length.GreaterThanOrEqualTo(1), "No results found");

        var gameResult = gamesInfo.Results.FirstOrDefault(r => r.Name == uniqueName);
        Assert.That(gameResult, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(gameResult.FinishTime, Is.EqualTo(gameResultInfo.FinishTime).Within(TimeSpan.FromMilliseconds(100)));
            Assert.That(gameResult.Duration, Is.EqualTo(gameResultInfo.Duration));
            Assert.That(gameResult.Name, Is.EqualTo(gameResultInfo.Name));
            Assert.That(gameResult.Package.Name, Is.EqualTo(gameResultInfo.Package.Name));
            CollectionAssert.AreEquivalent(gameResult.Package.Authors, gameResultInfo.Package.Authors);
            Assert.That(gameResult.Platform, Is.EqualTo(gameResultInfo.Platform));
            Assert.That(gameResult.Results.Count, Is.EqualTo(gameResultInfo.Results.Count));
            CollectionAssert.AreEquivalent(gameResult.Results.Keys, gameResultInfo.Results.Keys);
            CollectionAssert.AreEquivalent(gameResult.Results.Values, gameResultInfo.Results.Values);
            Assert.That(gameResult.Reviews, Is.EqualTo(gameResultInfo.Reviews));
            CollectionAssert.AreEquivalent(gameResult.Reviews.Keys, gameResultInfo.Reviews.Keys);
            CollectionAssert.AreEquivalent(gameResult.Reviews.Values, gameResultInfo.Reviews.Values);
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
            Assert.That(statistic.GameCount, Is.GreaterThan(2));
            Assert.That(statistic.TotalDuration, Is.GreaterThanOrEqualTo(TimeSpan.FromMinutes(40)));
        });

        var packages = await SIStatisticsClient.GetLatestTopPackagesAsync(
            new StatisticFilter
            {
                From = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)),
                To = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1)),
                Platform = GamePlatforms.Local,
            });

        Assert.That(packages, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(packages.Packages, Has.Length.GreaterThan(1));

            var testPackage = packages.Packages.FirstOrDefault(p => p.Package?.Name == "TestPackage");
            var testPackage2 = packages.Packages.FirstOrDefault(p => p.Package?.Name == "TestPackage 2");

            Assert.That(testPackage, Is.Not.Null);
            Assert.That(testPackage2, Is.Not.Null);
            Assert.That(testPackage!.GameCount, Is.GreaterThan(testPackage2!.GameCount));
        });
    }

    private Task AddPackageGamesAsync(string packageName)
    {
        var gameResultInfo2 = new GameResultInfo(new PackageInfo(packageName, "2", new[] { "TestAuthor 2" }))
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(10),
            Name = "_TEST_" + Guid.NewGuid().ToString(),
            Platform = GamePlatforms.Local,
            Results = new Dictionary<string, int>(),
            Reviews = new Dictionary<string, string>()
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
            QuestionReports = new QuestionReport[]
            {
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
            }
        };

        await SIStatisticsClient.SendGameReportAsync(gameReport);

        var questionInfo = await SIStatisticsClient.GetQuestionInfoAsync("Test theme", "Test question");

        Assert.That(questionInfo, Is.Not.Null);

        Assert.That(questionInfo.Entities.Count, Is.GreaterThanOrEqualTo(4));

        var accepted = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test accept");
        Assert.That(accepted, Is.Not.Null);

        Assert.That(accepted, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(accepted.RelationType, Is.EqualTo(RelationType.Accepted));

        var rejected = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test reject");
        Assert.That(rejected, Is.Not.Null);

        Assert.That(rejected, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(rejected.RelationType, Is.EqualTo(RelationType.Rejected));

        var appellated = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test apellate");
        Assert.That(appellated, Is.Not.Null);

        Assert.That(appellated, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(appellated.RelationType, Is.EqualTo(RelationType.Apellated));

        var complained = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Test complain");
        Assert.That(complained, Is.Not.Null);

        Assert.That(complained, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(complained.RelationType, Is.EqualTo(RelationType.Complained));
    }
}
