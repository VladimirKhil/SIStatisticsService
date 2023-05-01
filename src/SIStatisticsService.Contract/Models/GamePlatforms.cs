namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines hosting games platforms.
/// </summary>
[Flags]
public enum GamePlatforms
{
    /// <summary>
    /// Local (and local network) game.
    /// </summary>
    Local = 1,

    /// <summary>
    /// Game server game.
    /// </summary>
    GameServer = 2,
}
