namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines a collection of game results.
/// </summary>
public sealed class GamesResponse
{
    /// <summary>
    /// Game results.
    /// </summary>
    public GameResultInfo[] Results { get; set; } = Array.Empty<GameResultInfo>();
}
