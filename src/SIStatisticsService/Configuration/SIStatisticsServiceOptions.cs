namespace SIStatisticsService.Configuration;

/// <summary>
/// Provides options for SIStatisticsService service.
/// </summary>
public sealed class SIStatisticsServiceOptions
{
    public const string ConfigurationSectionName = "SIStatistics";

    /// <summary>
    /// Number of packages returned during statistic request.
    /// </summary>
    public int TopPackageCount { get; set; } = 10;

    /// <summary>
    /// Number of game results returned during statistic request.
    /// </summary>
    public int MaxResultCount { get; set; } = 100;

    /// <summary>
    /// Maximum allowed game duration.
    /// </summary>
    public TimeSpan MaximumGameDuration { get; set; } = TimeSpan.FromHours(10);

    /// <summary>
    /// Gets or sets the threshold for the number of events for answer to be returned to client.
    /// </summary>
    public int CollectedAnswersThreshold { get; set; } = 8;
}
