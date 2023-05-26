namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents packages statistics.
/// </summary>
public sealed class PackagesStatistic
{
    /// <summary>
    /// Individual packages statistics.
    /// </summary>
    public PackageStatistic[] Packages { get; set; } = Array.Empty<PackageStatistic>();
}
