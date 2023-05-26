using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.Contract;

/// <summary>
/// Defines a SIStatisticsService client.
/// </summary>
public interface ISIStatisticsServiceClient
{
    /// <summary>
    /// Sends package content to service.
    /// </summary>
    /// <param name="packageContentStream">Package content stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendPackageContentAsync(Stream packageContentStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads game report to service.
    /// </summary>
    /// <param name="gameReport">Game report.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendGameReportAsync(GameReport gameReport, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets latest games info.
    /// </summary>
    /// <param name="filter">Statistic filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<GamesResponse?> GetLatestGamesInfoAsync(StatisticFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets latest games cumulative statistic.
    /// </summary>
    /// <param name="filter">Statistic filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<GamesStatistic?> GetLatestGamesStatisticAsync(StatisticFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets latest top played packages.
    /// </summary>
    /// <param name="filter">Statistic filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PackagesStatistic?> GetLatestTopPackagesAsync(StatisticFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets question info.
    /// </summary>
    /// <param name="themeName">Question theme name.</param>
    /// <param name="questionText">Question text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<QuestionInfoResponse?> GetQuestionInfoAsync(string themeName, string questionText, CancellationToken cancellationToken = default);
}
