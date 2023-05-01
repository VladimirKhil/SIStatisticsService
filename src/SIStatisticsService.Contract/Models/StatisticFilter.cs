namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines a statistic request filter.
/// </summary>
public sealed class StatisticFilter
{
    /// <summary>
    /// Game platform.
    /// </summary>
    public GamePlatforms Platform { get; set; }

    /// <summary>
    /// Start date.
    /// </summary>
    public DateTimeOffset From { get; set; }

    /// <summary>
    /// End date.
    /// </summary>
    public DateTimeOffset To { get; set; }
}
