namespace SIStatisticsService.Database.Models.Games;

/// <summary>
/// Defines hosting games platforms.
/// </summary>
public enum GamePlatform
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
