namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines a request for package statistics.
/// </summary>
/// <param name="Name">Package name.</param>
/// <param name="Hash">Package hash.</param>
/// <param name="Authors">Package authors.</param>
public sealed record PackageStatsRequest(
    string Name,
    string Hash,
    string[] Authors);
