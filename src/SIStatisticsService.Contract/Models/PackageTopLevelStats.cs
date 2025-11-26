namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents top-level statistical data for a package.
/// </summary>
/// <param name="CompletedGameCount">The total number of finished games associated with the package.</param>
public sealed record PackageTopLevelStats(int CompletedGameCount);
