namespace SIStatisticsService.Database.Models.Games;

/// <summary>
/// Represents top-level statistical data for a package.
/// </summary>
/// <param name="CompletedGameCount">The total number of finished games associated with the package.</param>
public sealed record PackageTopLevelStatsModel(int CompletedGameCount);
