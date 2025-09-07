namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines request for top packages statistics.
/// </summary>
/// <param name="StatisticFilter">Statistic filter.</param>
/// <param name="Source">Optional primary source URI for the packages.</param>
/// <param name="FallbackSource">Optional fallback source URI used when primary source is not found.</param>
public sealed record TopPackagesRequest(StatisticFilter StatisticFilter, Uri? Source = null, Uri? FallbackSource = null);
