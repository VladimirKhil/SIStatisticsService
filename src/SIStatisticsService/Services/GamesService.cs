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
            from ps in connection.PackageSources.Where(ps => ps.PackageId == p.Id).DefaultIfEmpty()
            where ((int)g.Platform & (int)statisticFilter.Platform) > 0
                && g.FinishTime >= statisticFilter.From
                && g.FinishTime <= statisticFilter.To
                && (statisticFilter.LanguageCode == null || l != null && l.Code == statisticFilter.LanguageCode)
                && !p.Hidden
            select new GameResultInfo(
                new PackageInfo(p.Name, p.Hash, p.Authors, p.AuthorsContacts, ps != null ? new Uri(ps.Source) : null),
                l != null ? l.Code : null)
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
            from ps in connection.PackageSources.Where(ps => ps.PackageId == p.Id).DefaultIfEmpty()
            where g.Id == gameId && !p.Hidden
            select new GameResultInfo(
                new PackageInfo(p.Name, p.Hash, p.Authors, p.AuthorsContacts, ps != null ? new Uri(ps.Source) : null),
                l != null ? l.Code : null)
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

    public async Task<PackagesStatistic> GetPackagesStatisticAsync(
        StatisticFilter statisticFilter,
        Uri? source = null,
        CancellationToken cancellationToken = default)
    {
        var sourceTypeId = source != null ? source.GetStableHostHash() : (int?)null;

        // First, get the package statistics with game counts
        var packageStatsQuery =
            from p in connection.Packages
            join g in connection.Games on p.Id equals g.PackageId
            from l in connection.Languages.Where(l => l.Id == g.LanguageId).DefaultIfEmpty()
            where ((int)g.Platform & (int)statisticFilter.Platform) > 0
                && g.FinishTime >= statisticFilter.From
                && g.FinishTime <= statisticFilter.To
                && (statisticFilter.LanguageCode == null || l != null && l.Code == statisticFilter.LanguageCode)
                && !p.Hidden
            group g by new { p.Id, p.Name, p.Hash, p.Authors, p.AuthorsContacts } into packageGroup
            orderby packageGroup.Count() descending
            select new
            {
                PackageId = packageGroup.Key.Id,
                PackageName = packageGroup.Key.Name,
                PackageHash = packageGroup.Key.Hash,
                PackageAuthors = packageGroup.Key.Authors,
                PackageAuthorsContacts = packageGroup.Key.AuthorsContacts,
                GameCount = packageGroup.Count()
            };

        var maxCount = GetMaxItemCount(statisticFilter, _options.TopPackageCount);
        var packageStats = await packageStatsQuery.Take(maxCount).ToArrayAsync(cancellationToken);

        // Then, get the package sources for the selected packages
        var packageIds = packageStats.Select(ps => ps.PackageId).ToArray();
        var packageSources = await (
            from ps in connection.PackageSources
            where packageIds.Contains(ps.PackageId)
                && (sourceTypeId == null || ps.SourceTypeId == sourceTypeId)
            select new { ps.PackageId, ps.Source }
        ).ToArrayAsync(cancellationToken);

        // Create a lookup for package sources
        var sourcesByPackageId = packageSources.ToLookup(ps => ps.PackageId);

        // Build the final result
        var packages = packageStats.Select(ps =>
        {
            var packageSource = sourcesByPackageId[ps.PackageId].FirstOrDefault();
            return new PackageStatistic
            {
                Package = new PackageInfo(
                    ps.PackageName,
                    ps.PackageHash,
                    ps.PackageAuthors,
                    ps.PackageAuthorsContacts,
                    packageSource != null ? new Uri(packageSource.Source) : null),
                GameCount = ps.GameCount
            };
        }).ToArray();

        return new PackagesStatistic
        {
            Packages = packages
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

        var packageId = (await connection.Packages.FirstAsync(
            p => p.Name == packageInfo.Name && p.Hash == packageInfo.Hash && p.Authors == packageInfo.Authors,
            token: cancellationToken)).Id;

        if (packageInfo.Source != null)
        {
            var sourceTypeId = packageInfo.Source.GetStableHostHash();

            await connection.PackageSources.InsertOrUpdateAsync(
                () => new PackageSourceModel
                {
                    PackageId = packageId,
                    SourceTypeId = sourceTypeId,
                    Source = packageInfo.Source.ToString()
                },
                p => new PackageSourceModel
                {
                    PackageId = packageId,
                    SourceTypeId = sourceTypeId,
                    Source = p.Source,
                },
                () => new PackageSourceModel
                {
                    PackageId = packageId,
                    SourceTypeId = sourceTypeId,
                },
                cancellationToken);
        }

        return packageId;
    }

    private static int GetMaxItemCount(StatisticFilter statisticFilter, int defaultValue) =>
        statisticFilter.Count.HasValue && statisticFilter.Count.Value > 0 ? Math.Min(statisticFilter.Count.Value, defaultValue) : defaultValue;
}
