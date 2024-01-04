namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines well-known SIStatistic service error codes.
/// </summary>
public enum WellKnownSIStatisticServiceErrorCode
{
    /// <summary>
    /// Unknown error.
    /// </summary>
    Unknown,

    /// <summary>
    /// Package file not found.
    /// </summary>
    PackageFileNotFound,

    /// <summary>
    /// Invalid FinishTime value.
    /// </summary>
    InvalidFinishTime,

    /// <summary>
    /// Game info not found.
    /// </summary>
    GameInfoNotFound,

    /// <summary>
    /// Unsupported game platform.
    /// </summary>
    UnsupportedPlatform,

    /// <summary>
    /// Invalid Duration value.
    /// </summary>
    InvalidDuration,
}
