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
public sealed class GamesService(
    SIStatisticsDbConnection connection,
    IOptions<SIStatisticsServiceOptions> options,
    OtelMetrics metrics) : IGamesService
{
    private readonly SIStatisticsServiceOptions _options = options.Value;
    
    public async Task<GameResultInfo[]> GetGamesByFilterAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query =
            from g in connection.Games
            from p in connection.Packages.Where(p => p.Id == g.PackageId)
            from l in connection.Languages.Where(l => l.Id == g.LanguageId).DefaultIfEmpty()
            where ((int)g.Platform & (int)statisticFilter.Platform) > 0
                && g.FinishTime >= statisticFilter.From
                && g.FinishTime <= statisticFilter.To
                && (statisticFilter.LanguageCode == null || l != null && l.Code == statisticFilter.LanguageCode)
                && !p.Hidden
            select new GameResultInfo(new PackageInfo(p.Name, p.Hash, p.Authors, p.AuthorsContacts), l != null ? l.Code : null)
            {
                Name = g.Name,
                Duration = g.Duration,
                FinishTime = g.FinishTime,
                Platform = g.Platform == GamePlatform.GameServer ? GamePlatforms.GameServer : GamePlatforms.Local,
                Results = g.Scores,
                Reviews = g.Reviews
            };

        var maxCount = GetMaxItemCount(statisticFilter, _options.MaxResultCount);

        return await query.OrderByDescending(gri => gri.FinishTime).Take(maxCount).ToArrayAsync(cancellationToken);
    }

    public async Task<GameResultInfo?> TryGetGameAsync(int gameId, CancellationToken cancellationToken)
    {
        var query =
            from g in connection.Games
            from p in connection.Packages.Where(p => p.Id == g.PackageId)
            from l in connection.Languages.Where(l => l.Id == g.LanguageId).DefaultIfEmpty()
            where g.Id == gameId && !p.Hidden
            select new GameResultInfo(new PackageInfo(p.Name, p.Hash, p.Authors, p.AuthorsContacts), l != null ? l.Code : null)
            {
                Name = g.Name,
                Duration = g.Duration,
                FinishTime = g.FinishTime,
                Platform = g.Platform == GamePlatform.GameServer ? GamePlatforms.GameServer : GamePlatforms.Local,
                Results = g.Scores,
                Reviews = g.Reviews
            };

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GamesStatistic> GetGamesStatisticAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken)
    {
        var query =
            from g in connection.Games
            from l in connection.Languages.Where(l => l.Id == g.LanguageId).DefaultIfEmpty()
            where ((int)g.Platform & (int)statisticFilter.Platform) > 0
                && g.FinishTime >= statisticFilter.From
                && g.FinishTime <= statisticFilter.To
                && (statisticFilter.LanguageCode == null || l != null && l.Code == statisticFilter.LanguageCode)
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
            from p in connection.Packages
            join g in connection.Games on p.Id equals g.PackageId into packageGames
            from pg in packageGames.DefaultIfEmpty()
            from l in connection.Languages.Where(l => l.Id == pg.LanguageId).DefaultIfEmpty()
            where ((int)pg.Platform & (int)statisticFilter.Platform) > 0
                && pg.FinishTime >= statisticFilter.From
                && pg.FinishTime <= statisticFilter.To
                && (statisticFilter.LanguageCode == null || l != null && l.Code == statisticFilter.LanguageCode)
                && !p.Hidden
            group p by new { p.Id, p.Name, p.Hash, p.Authors, p.AuthorsContacts } into g
            orderby g.Count() descending
            select new PackageStatistic
            {
                Package = new PackageInfo(g.Key.Name, g.Key.Hash, g.Key.Authors, g.Key.AuthorsContacts),
                GameCount = g.Count()
            };

        var maxCount = GetMaxItemCount(statisticFilter, _options.TopPackageCount);

        return new PackagesStatistic
        {
            Packages = await query.Take(maxCount).ToArrayAsync(cancellationToken)
        };
    }

    public async Task<int> AddGameResultAsync(GameResultInfo gameResult, CancellationToken cancellationToken)
    {
        if (gameResult.FinishTime == DateTimeOffset.MinValue)
        {
            throw new ArgumentException("Invalid FinishTime");
        }

        using var tx = await connection.BeginTransactionAsync(cancellationToken);
        var packageId = await InsertOrUpdatePackageAsync(gameResult.Package, cancellationToken);
        var languageId = await GetLanguageIdAsync(gameResult.LanguageCode, cancellationToken);
        var gameId = await InsertGameAsync(gameResult, packageId, languageId, cancellationToken);
        await tx.CommitAsync(cancellationToken);

        metrics.AddGameReport();

        return gameId;
    }

    private async Task<int?> GetLanguageIdAsync(string? languageCode, CancellationToken cancellationToken)
    {
        if (languageCode == null)
        {
            return null;
        }

        return (await connection.Languages.FirstOrDefaultAsync(l => l.Code == languageCode, cancellationToken))?.Id;
    }

    private Task<int> InsertGameAsync(GameResultInfo gameResult, int packageId, int? languageId, CancellationToken cancellationToken) =>
        connection.Games.InsertWithInt32IdentityAsync(
            () => new GameModel
            {
                Name = gameResult.Name,
                FinishTime = gameResult.FinishTime,
                Duration = gameResult.Duration,
                Platform = gameResult.Platform == GamePlatforms.GameServer ? GamePlatform.GameServer : GamePlatform.Local,
                PackageId = packageId,
                Scores = gameResult.Results,
                Reviews = gameResult.Reviews,
                LanguageId = languageId
            },
            cancellationToken
        );

    private async Task<int> InsertOrUpdatePackageAsync(PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        await connection.Packages.InsertOrUpdateAsync(
            () => new PackageModel
            {
                Name = packageInfo.Name,
                Hash = packageInfo.Hash,
                Authors = packageInfo.Authors,
                AuthorsContacts = packageInfo.AuthorsContacts
            },
            p => new PackageModel
            {
                Name = packageInfo.Name,
                Hash = packageInfo.Hash,
                Authors = packageInfo.Authors,
                AuthorsContacts = p.AuthorsContacts ?? packageInfo.AuthorsContacts
            },
            () => new PackageModel
            {
                Name = packageInfo.Name,
                Hash = packageInfo.Hash,
                Authors = packageInfo.Authors
            },
            cancellationToken);

        return (await connection.Packages.FirstAsync(
            p => p.Name == packageInfo.Name && p.Hash == packageInfo.Hash && p.Authors == packageInfo.Authors,
            token: cancellationToken)).Id;
    }

    private static int GetMaxItemCount(StatisticFilter statisticFilter, int defaultValue) =>
        statisticFilter.Count.HasValue && statisticFilter.Count.Value > 0 ? Math.Min(statisticFilter.Count.Value, defaultValue) : defaultValue;
}
