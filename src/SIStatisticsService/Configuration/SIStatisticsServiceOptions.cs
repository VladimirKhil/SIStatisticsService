namespace SIStatisticsService.Configuration;

/// <summary>
/// Provides options for SIStatisticsService service.
/// </summary>
public sealed class SIStatisticsServiceOptions
{
    public const string ConfigurationSectionName = "SIStatistics";

    public int TopPackageCount { get; set; } = 10;

    public int MaxResultCount { get; set; } = 100;
}
