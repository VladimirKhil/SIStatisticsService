namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Request to get package info and optionally its statistics.
/// </summary>
/// <param name="Name">Package name.</param>
/// <param name="Hash">Package hash.</param>
/// <param name="Authors">Package authors.</param>
/// <param name="Source">Preferred package source.</param>
/// <param name="IncludeStats">Whether to include package statistics.</param>
public sealed record PackageInfoRequest(
    string Name,
    string Hash,
    string[] Authors,
    Uri? Source = null,
    bool IncludeStats = false);
