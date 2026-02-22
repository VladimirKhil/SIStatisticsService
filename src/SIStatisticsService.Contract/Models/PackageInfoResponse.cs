namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Response for single package info request. Contains package info and optional statistics.
/// </summary>
/// <param name="Package">Package information.</param>
/// <param name="Stats">Optional package statistics.</param>
public sealed record PackageInfoResponse(
    PackageInfo Package,
    PackageStats? Stats = null);
