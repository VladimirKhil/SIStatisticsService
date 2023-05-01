namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines a SIStatisticService error.
/// </summary>
public sealed class SIStatisticServiceError
{
    /// <summary>
    /// Error code.
    /// </summary>
    public WellKnownSIStatisticServiceErrorCode ErrorCode { get; set; }
}
