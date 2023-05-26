namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents package statistics.
/// </summary>
public sealed class PackageStatistic
{
    /// <summary>
    /// Package info.
    /// </summary>
    public PackageInfo? Package { get; set; }

    /// <summary>
    /// Number of games played with this package.
    /// </summary>
    public int GameCount { get; set; }
}
