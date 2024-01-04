using LinqToDB;
using Microsoft.Extensions.Options;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using SIStatisticsService.Database.Models.Games;
using SIStatisticsService.Helpers;
using SIStatisticsService.Metrics;

namespace SIStatisticsService.Services;

/// <inheritdoc cref="IGamesService" />
public sealed class GamesService : IGamesService
{
    private readonly SIStatisticsDbConnection _connection;
    private readonly SIStatisticsServiceOptions _options;
    private readonly OtelMetrics _metrics;

    public GamesService(
        SIStatisticsDbConnection connection,
        IOptions<SIStatisticsServiceOptions> options,
        OtelMetrics metrics)
    {
        _connection = connection;
        _options = options.Value;
        _metrics = metrics;
    }

    public Task<GameResultInfo[]> GetGamesByFilterAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query =
            from g in _connection.Games
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

        return query.OrderByDescending(gri => gri.FinishTime).Take(_options.MaxResultCount).ToArrayAsync(cancellationToken);
    }

    public async Task<GamesStatistic> GetGamesStatisticAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query =
            from g in _connection.Games
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
            TotalDuration = results.Select(r => r.Duration).Aggregate(TimeSpan.Zero, TimeSpanHelper.AddTimeSpan)
        };
    }

    public async Task<PackagesStatistic> GetPackagesStatisticAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query =
            from p in _connection.Packages
            join g in _connection.Games on p.Id equals g.PackageId into packageGames
            from pg in packageGames.DefaultIfEmpty()
            where ((int)pg.Platform & (int)statisticFilter.Platform) > 0
                && pg.FinishTime >= statisticFilter.From
                && pg.FinishTime <= statisticFilter.To
            group p by new { p.Id, p.Name, p.Hash, p.Authors } into g
            orderby g.Count() descending
            select new PackageStatistic
            {
                Package = new PackageInfo
                {
                    Name = g.Key.Name,
                    Hash = g.Key.Hash,
                    Authors = g.Key.Authors
                },
                GameCount = g.Count()
            };

        return new PackagesStatistic
        {
            Packages = await query.Take(_options.TopPackageCount).ToArrayAsync(cancellationToken)
        };
    }

    public async Task AddGameResultAsync(GameResultInfo gameResult, CancellationToken cancellationToken)
    {
        using var tx = await _connection.BeginTransactionAsync(cancellationToken);
        var packageId = await InsertOrUpdatePackageAsync(gameResult.Package, cancellationToken);
        await InsertGameAsync(gameResult, packageId, cancellationToken);
        await tx.CommitAsync(cancellationToken);

        _metrics.AddGameReport();
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
        await _connection.Packages.InsertOrUpdateAsync(
            () => new PackageModel
            {
                Name = packageInfo.Name,
                Hash = packageInfo.Hash,
                Authors = packageInfo.Authors
            },
            null,
            () => new PackageModel
            {
                Name = packageInfo.Name,
                Hash = packageInfo.Hash,
                Authors = packageInfo.Authors
            },
            cancellationToken);

        return (await _connection.Packages.FirstAsync(
            p => p.Name == packageInfo.Name && p.Hash == packageInfo.Hash && p.Authors == packageInfo.Authors,
            token: cancellationToken)).Id;
    }
}
