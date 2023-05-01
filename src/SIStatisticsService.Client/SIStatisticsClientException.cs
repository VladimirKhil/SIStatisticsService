using SIStatisticsService.Contract.Models;
using System.Net;

namespace SIStatisticsService.Client;

/// <summary>
/// Defines a SIStatistics client exception.
/// </summary>
public sealed class SIStatisticsClientException : Exception
{
    /// <summary>
    /// Error code.
    /// </summary>
    public WellKnownSIStatisticServiceErrorCode ErrorCode { get; set; }

    /// <summary>
    /// HTTP error status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    public SIStatisticsClientException() { }

    public SIStatisticsClientException(string message) : base(message) { }
}
