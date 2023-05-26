using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.Contracts;

/// <summary>
/// Provides API for working with games.
/// </summary>
public interface IGamesService
{
    /// <summary>
    /// Gets games history.
    /// </summary>
    /// <param name="statisticFilter">History filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<GameResultInfo[]> GetGamesByFilterAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken);

    /// <summary>
    /// Adds new game result record.
    /// </summary>
    /// <param name="gameResult">Game result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddGameResultAsync(GameResultInfo gameResult, CancellationToken cancellationToken);

    /// <summary>
    /// Gets cumulative latest games statistic.
    /// </summary>
    /// <param name="statisticFilter">Statistic filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<GamesStatistic> GetGamesStatisticAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken);

    /// <summary>
    /// Get cumulative latest played packages statistic.
    /// </summary>
    /// <param name="statisticFilter">Statistic filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PackagesStatistic> GetPackagesStatisticAsync(StatisticFilter statisticFilter, CancellationToken cancellationToken);
}
