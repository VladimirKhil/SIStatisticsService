﻿using LinqToDB;
using Microsoft.Extensions.Options;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using SIStatisticsService.Database.Models.Games;

namespace SIStatisticsService.Services;

/// <inheritdoc cref="IGamesService" />
public sealed class GamesService : IGamesService
{
    private readonly SIStatisticsDbConnection _connection;
    private readonly SIStatisticsServiceOptions _options;

    public GamesService(SIStatisticsDbConnection connection, IOptions<SIStatisticsServiceOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }

    public Task<GameResultInfo[]> GetGamesByFilterAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query = from g in _connection.Games
                    from p in _connection.Packages.Where(p => p.Id == g.PackageId)
                    where ((int)g.Platform & (int)statisticFilter.Platform) > 0
                        && g.FinishTime >= statisticFilter.From
                        && g.FinishTime <= statisticFilter.To
                    select new GameResultInfo
                    {
                        Name = g.Name,
                        Duration = g.Duration,
                        FinishTime = g.FinishTime,
                        Platform = g.Platform == GamePlatform.GameServer ? GamePlatforms.GameServer : GamePlatforms.Local,
                        Package = new PackageInfo
                        {
                            Name = p.Name,
                            Hash = p.Hash,
                            Authors = p.Authors,
                        },
                        Results = g.Scores,
                        Reviews = g.Reviews
                    };

        return query.ToArrayAsync(cancellationToken);
    }

    public async Task<GamesStatistic> GetGamesStatisticAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query = from g in _connection.Games
                    where ((int)g.Platform & (int)statisticFilter.Platform) > 0
                        && g.FinishTime >= statisticFilter.From
                        && g.FinishTime <= statisticFilter.To
                    select new
                    {
                        g.Duration
                    };

        var results = await query.ToArrayAsync(cancellationToken);

        return new GamesStatistic
        {
            GameCount = results.Length,
            TotalDuration = results.Select(r => r.Duration).Aggregate(TimeSpan.Zero, (value, result) => value + result)
        };
    }

    public async Task<PackageStatistic> GetPackagesStatisticAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query = from g in _connection.Games
                    from p in _connection.Packages.Where(p => p.Id == g.PackageId)
                    where ((int)g.Platform & (int)statisticFilter.Platform) > 0
                        && g.FinishTime >= statisticFilter.From
                        && g.FinishTime <= statisticFilter.To
                    select new PackageInfo
                    {
                        Name = p.Name,
                        Hash = p.Hash,
                        Authors = p.Authors
                    };

        return new PackageStatistic
        {
            Packages = await query.Take(_options.TopPackagesCount).ToArrayAsync(cancellationToken)
        };
    }

    public async Task AddGameResultAsync(GameResultInfo gameResult, CancellationToken cancellationToken)
    {
        using var tx = await _connection.BeginTransactionAsync(cancellationToken);
        var packageId = await InsertOrUpdatePackageAsync(gameResult.Package, cancellationToken);
        await InsertGameAsync(gameResult, packageId, cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private Task<int> InsertGameAsync(GameResultInfo gameResult, int packageId, CancellationToken cancellationToken) =>
        _connection.Games.InsertWithInt32IdentityAsync(
            () => new GameModel
            {
                Name = gameResult.Name,
                FinishTime = gameResult.FinishTime,
                Duration = gameResult.Duration,
                Platform = gameResult.Platform == GamePlatforms.GameServer ? GamePlatform.GameServer : GamePlatform.Local,
                PackageId = packageId,
                Scores = gameResult.Results,
                Reviews = gameResult.Reviews
            },
            cancellationToken
        );

    private async Task<int> InsertOrUpdatePackageAsync(PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var package = await _connection.Packages
            .Where(p => p.Name == packageInfo.Name && p.Hash == packageInfo.Hash)
            .FirstOrDefaultAsync(cancellationToken);

        if (package == null)
        {
            var packageId = (int?)await _connection.Packages.InsertWithIdentityAsync(
                () => new PackageModel
                {
                    Name = packageInfo.Name,
                    Hash = packageInfo.Hash,
                    Authors = packageInfo.Authors
                },
                cancellationToken);

            return packageId == null ? throw new Exception($"Could not insert package {packageInfo.Name}") : packageId.Value;
        }

        return package.Id;
    }
}