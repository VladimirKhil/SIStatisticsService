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
            select new GameResultInfo(
                new PackageInfo(p.Name, p.Hash, p.Authors, p.AuthorsContacts, null),
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
        TopPackagesRequest packagesRequest,
        CancellationToken cancellationToken = default)
    {
        var statisticFilter = packagesRequest.StatisticFilter;
        var sourceTypeId = packagesRequest.Source != null ? packagesRequest.Source.GetStableHostHash() : (int?)null;
        var fallbackSourceTypeId = packagesRequest.FallbackSource != null ? packagesRequest.FallbackSource.GetStableHostHash() : (int?)null;

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
                && (sourceTypeId == null || fallbackSourceTypeId == null || ps.SourceTypeId == sourceTypeId || ps.SourceTypeId == fallbackSourceTypeId)
            select new { ps.PackageId, ps.SourceTypeId, ps.Source }
        ).ToArrayAsync(cancellationToken);

        // Create a lookup for package sources
        var sourcesByPackageId = packageSources.ToLookup(ps => ps.PackageId);

        // Build the final result
        var packages = packageStats.Select(ps =>
        {
            var packageSources = sourcesByPackageId[ps.PackageId];
            
            // Find the appropriate source to use
            Uri? finalSource = null;
            
            if (sourceTypeId != null)
            {
                // First try to find the primary source
                var primarySource = packageSources.FirstOrDefault(s => s.SourceTypeId == sourceTypeId);
                if (primarySource != null)
                {
                    finalSource = new Uri(primarySource.Source);
                }
                else if (fallbackSourceTypeId != null)
                {
                    // If primary source not found, try fallback source
                    var fallbackSourceMatch = packageSources.FirstOrDefault(s => s.SourceTypeId == fallbackSourceTypeId);
                    if (fallbackSourceMatch != null)
                    {
                        finalSource = new Uri(fallbackSourceMatch.Source);
                    }
                }
            }
            else
            {
                // If no source filter is specified, return the first available source
                var firstSource = packageSources.FirstOrDefault();
                if (firstSource != null)
                {
                    finalSource = new Uri(firstSource.Source);
                }
            }

            return new PackageStatistic
            {
                Package = new PackageInfo(
                    ps.PackageName,
                    ps.PackageHash,
                    ps.PackageAuthors,
                    ps.PackageAuthorsContacts,
                    finalSource),
                GameCount = ps.GameCount
            };
        }).ToArray();

        return new PackagesStatistic
        {
            Packages = packages
        };
    }

    public async Task<int> AddGameResultAsync(
        GameResultInfo gameResult,
        PackageStats? packageStats,
        CancellationToken cancellationToken)
    {
        if (gameResult.FinishTime == DateTimeOffset.MinValue)
        {
            throw new ArgumentException("Invalid FinishTime");
        }

        using var tx = await connection.BeginTransactionAsync(cancellationToken);
        var packageId = await InsertOrUpdatePackageAsync(gameResult.Package, packageStats, cancellationToken);
        var languageId = await GetLanguageIdAsync(gameResult.LanguageCode, cancellationToken);
        var gameId = await InsertGameAsync(gameResult, packageId, languageId, cancellationToken);
        await tx.CommitAsync(cancellationToken);

        metrics.AddGameReport();

        return gameId;
    }

    public async Task<PackageStats?> GetPackageStatsAsync(PackageStatsRequest request, CancellationToken cancellationToken = default)
    {
        // Find the package first
        var package = await connection.Packages
            .FirstOrDefaultAsync(p => p.Name == request.Name &&
                                    p.Hash == request.Hash &&
                                    p.Authors == request.Authors,
                                cancellationToken);

        if (package == null || package.Hidden || package.Stats == null)
        {
            return null;
        }

        var stats = package.Stats;
        var topLevelStats = new PackageTopLevelStats(stats.TopLevelStats.StartedGameCount, stats.TopLevelStats.CompletedGameCount);

        var questionStats = stats.QuestionStats.ToDictionary(
            qs => qs.Key,
            qs => new QuestionStats(
                qs.Value.ShownCount,
                qs.Value.PlayerSeenCount,
                qs.Value.AnsweredCount,
                qs.Value.CorrectCount,
                qs.Value.WrongCount));

        return new PackageStats(topLevelStats, questionStats);
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

    private async Task<int> InsertOrUpdatePackageAsync(
        PackageInfo packageInfo,
        PackageStats? packageStats,
        CancellationToken cancellationToken)
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

        if (packageStats != null)
        {
            await UpdatePackageStatsAsync(packageId, packageStats, cancellationToken);
        }

        return packageId;
    }

    private async Task UpdatePackageStatsAsync(int packageId, PackageStats packageStats, CancellationToken cancellationToken)
    {
        // Get the current package with its stats
        var existingPackage = await connection.Packages.FirstOrDefaultAsync(p => p.Id == packageId, cancellationToken);

        if (existingPackage == null)
        {
            return;
        }

        PackageStatsModel newStatsModel;

        if (existingPackage.Stats == null)
        {
            // No existing stats, create new ones
            var topLevelStats = new PackageTopLevelStatsModel(1, packageStats.TopLevelStats.CompletedGameCount);

            var questionStats = packageStats.QuestionStats.ToDictionary(
                kvp => kvp.Key,
                kvp => new QuestionStatsModel(
                    kvp.Value.ShownCount,
                    kvp.Value.PlayerSeenCount,
                    kvp.Value.CorrectCount + kvp.Value.WrongCount > 0 ? kvp.Value.ShownCount : 0,
                    kvp.Value.CorrectCount,
                    kvp.Value.WrongCount));

            newStatsModel = new PackageStatsModel(topLevelStats, questionStats);
        }
        else
        {
            // Merge existing stats with new stats
            var existingStats = existingPackage.Stats;

            // Merge top-level stats by adding completed game counts
            var mergedTopLevelStats = new PackageTopLevelStatsModel(
                existingStats.TopLevelStats.StartedGameCount + 1,
                existingStats.TopLevelStats.CompletedGameCount + packageStats.TopLevelStats.CompletedGameCount);

            // Merge question stats
            var mergedQuestionStats = new Dictionary<string, QuestionStatsModel>(existingStats.QuestionStats);

            foreach (var newQuestionStat in packageStats.QuestionStats)
            {
                var questionKey = newQuestionStat.Key;
                var newStats = newQuestionStat.Value;

                if (mergedQuestionStats.TryGetValue(questionKey, out var existingQuestionStats))
                {
                    // Merge existing question stats with new stats by adding counts
                    mergedQuestionStats[questionKey] = new QuestionStatsModel(
                        existingQuestionStats.ShownCount + newStats.ShownCount,
                        existingQuestionStats.PlayerSeenCount + newStats.PlayerSeenCount,
                        newStats.CorrectCount + newStats.WrongCount > 0
                            ? existingQuestionStats.ShownCount + newStats.ShownCount
                            : existingQuestionStats.ShownCount,
                        existingQuestionStats.CorrectCount + newStats.CorrectCount,
                        existingQuestionStats.WrongCount + newStats.WrongCount);
                }
                else
                {
                    // Add new question stats
                    mergedQuestionStats[questionKey] = new QuestionStatsModel(
                        newStats.ShownCount,
                        newStats.PlayerSeenCount,
                        newStats.CorrectCount + newStats.WrongCount > 0 ? newStats.ShownCount : 0,
                        newStats.CorrectCount,
                        newStats.WrongCount);
                }
            }

            newStatsModel = new PackageStatsModel(mergedTopLevelStats, mergedQuestionStats);
        }

        // Update the package with the new stats
        await connection.Packages
            .Where(p => p.Id == packageId)
            .UpdateAsync(p => new PackageModel { Stats = newStatsModel }, cancellationToken);
    }

    private static int GetMaxItemCount(StatisticFilter statisticFilter, int defaultValue) =>
        statisticFilter.Count.HasValue && statisticFilter.Count.Value > 0 ? Math.Min(statisticFilter.Count.Value, defaultValue) : defaultValue;
}
