using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.Client;

/// <summary>
/// Provides options for SIStatisticsService client.
/// </summary>
public sealed class SIStatisticsClientOptions
{
    /// <summary>
    /// Name of the configuration section holding these options.
    /// </summary>
    public const string ConfigurationSectionName = "SIStatisticsServiceClient";
    
    public const int DefaultRetryCount = 3;

    /// <summary>
    /// SIStatisticsService Uri.
    /// </summary>
    public Uri? ServiceUri { get; set; }
    
    /// <summary>
    /// Client secret for sending <see cref="GamePlatforms.GameServer" /> statistic.
    /// </summary>
    /// <remarks>
    /// Arbitrary client does not have rights to upload game server statistic.
    /// </remarks>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Retry count policy.
    /// </summary>
    public int RetryCount { get; set; } = DefaultRetryCount;
}
