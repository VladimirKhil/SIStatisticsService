﻿using SIStatisticsService.Client;
using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.IntegrationTests;

internal sealed class GamesApiTests : TestsBase
{
    [Test]
    public void SendLocalGameStatistic_InvalidFinishTime_Fail()
    {
        var exception = Assert.ThrowsAsync<SIStatisticsClientException>(() => SIStatisticsClient.SendGameReportAsync(new GameReport
        {
            Info = new GameResultInfo
            {
                FinishTime = new DateTimeOffset(2023, 3, 18, 12, 0, 0, TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(40),
                Name = "TestGame",
                Package = new PackageInfo
                {
                    Name = "TestPackage",
                    Authors = new[] { "TestAuthor" }
                },
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
    public async Task SendServerGameStatistic_Ok()
    {
        var uniqueName = "_TEST_" + Guid.NewGuid().ToString();

        var gameResultInfo = new GameResultInfo
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(40),
            Name = uniqueName,
            Package = new PackageInfo
            {
                Name = "TestPackage",
                Hash = "1",
                Authors = new[] { "TestAuthor" }
            },
            Platform = GamePlatforms.GameServer,
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

        var gameResultInfo2 = new GameResultInfo
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromMinutes(10),
            Name = uniqueName + "_2",
            Package = new PackageInfo
            {
                Name = "TestPackage 2",
                Hash = "2",
                Authors = new[] { "TestAuthor 2" }
            },
            Platform = GamePlatforms.GameServer,
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

        var gameResultInfo3 = new GameResultInfo
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromHours(1),
            Name = uniqueName + "_3",
            Package = new PackageInfo
            {
                Name = "TestPackage",
                Hash = "1",
                Authors = new[] { "TestAuthor" }
            },
            Platform = GamePlatforms.GameServer,
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

        var gameResultInfo4 = new GameResultInfo
        {
            FinishTime = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromHours(2),
            Name = uniqueName + "_4",
            Package = new PackageInfo
            {
                Name = "TestPackage",
                Hash = "1",
                Authors = new[] { "TestAuthor" }
            },
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
                Platform = GamePlatforms.GameServer,
            });

        Assert.That(gamesInfo, Is.Not.Null);
        Assert.That(gamesInfo.Results, Has.Length.GreaterThanOrEqualTo(1));

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
                Platform = GamePlatforms.GameServer,
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
                Platform = GamePlatforms.GameServer,
            });

        Assert.That(packages, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(packages.Packages, Has.Length.GreaterThan(1));
            Assert.That(packages.Packages[0].Name, Is.EqualTo("TestPackage"));
            Assert.That(packages.Packages[1].Name, Is.EqualTo("TestPackage 2"));
        });
    }
}