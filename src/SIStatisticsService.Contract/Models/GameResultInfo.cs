namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines game result info.
/// </summary>
/// <param name="Package">Game package info.</param>
/// <param name="LanguageCode">Game language code.</param>
public sealed record GameResultInfo(PackageInfo Package, string? LanguageCode = null)
{
    /// <summary>
    /// Game name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Game platform.
    /// </summary>
    public GamePlatforms Platform { get; set; }

    /// <summary>
    /// Game finish time.
    /// </summary>
    public DateTimeOffset FinishTime { get; set; }

    /// <summary>
    /// Game duration.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Game results: player names and their scores.
    /// </summary>
    public Dictionary<string, int> Results { get; init; } = new();

    /// <summary>
    /// Player reviews.
    /// </summary>
    public Dictionary<string, string> Reviews { get; init; } = new();
}
