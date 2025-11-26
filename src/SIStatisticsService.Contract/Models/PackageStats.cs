namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents statistical data for a package.
/// </summary>
/// <param name="TopLevelStats">Top-level statistical data for the package.</param>
/// <param name="QuestionStats">A dictionary containing statistics for each question in the package.</param>
public sealed record PackageStats(
    PackageTopLevelStats TopLevelStats,
    Dictionary<string, QuestionStats> QuestionStats);
