namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents package statistics.
/// </summary>
public sealed class PackageStatistic
{
    /// <summary>
    /// Statistic packages.
    /// </summary>
    public PackageInfo[] Packages { get; set; } = Array.Empty<PackageInfo>();
}
