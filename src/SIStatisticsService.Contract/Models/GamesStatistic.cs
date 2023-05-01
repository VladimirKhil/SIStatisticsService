namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Provides common games statistic info for a period.
/// </summary>
public sealed class GamesStatistic
{
    /// <summary>
    /// Finished games count.
    /// </summary>
    public int GameCount { get; set; }

    /// <summary>
    /// Finished games total duration.
    /// </summary>
    public TimeSpan TotalDuration { get; set; }
}
