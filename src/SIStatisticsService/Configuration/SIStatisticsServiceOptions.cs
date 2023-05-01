namespace SIStatisticsService.Configuration;

/// <summary>
/// Provides options for SIStatisticsService service.
/// </summary>
public sealed class SIStatisticsServiceOptions
{
    public const string ConfigurationSectionName = "SIStatistics";

    public int TopPackagesCount { get; set; } = 10;
}
