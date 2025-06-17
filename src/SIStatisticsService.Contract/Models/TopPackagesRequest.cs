namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines request for top packages statistics.
/// </summary>
/// <param name="StatisticFilter">Statistic filter.</param>
/// <param name="Source">Optional source URI for the packages.</param>
public sealed record TopPackagesRequest(StatisticFilter StatisticFilter, Uri? Source = null);
