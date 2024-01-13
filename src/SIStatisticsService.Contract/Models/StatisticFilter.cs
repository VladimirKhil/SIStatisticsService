namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines a statistic request filter.
/// </summary>
public sealed class StatisticFilter
{
    /// <summary>
    /// Game platform.
    /// </summary>
    public GamePlatforms Platform { get; set; } = GamePlatforms.GameServer;

    /// <summary>
    /// Start date.
    /// </summary>
    public DateTimeOffset From { get; set; } = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1));

    /// <summary>
    /// End date.
    /// </summary>
    public DateTimeOffset To { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Game language code.
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Maximum result count.
    /// </summary>
    public int? Count { get; set; }
}
