﻿using Microsoft.AspNetCore.Mvc;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Exceptions;

namespace SIStatisticsService.Controllers;

/// <summary>
/// Provides API for working with games.
/// </summary>
[Route("api/v1/games")]
[ApiController]
public sealed class GamesController : ControllerBase
{
    private readonly IGamesService _gamesService;

    public GamesController(IGamesService gamesService) => _gamesService = gamesService;

    [HttpGet("results")]
    public async Task<ActionResult<GamesResponse>> GetLatestGamesInfoAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken)
    {
        var games = await _gamesService.GetGamesByFilterAsync(statisticFilter, cancellationToken);

        return Ok(new GamesResponse { Results = games });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<GamesStatistic>> GetLatestGamesStatisticAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken)
    {
        var gamesStatistic = await _gamesService.GetGamesStatisticAsync(statisticFilter, cancellationToken);

        return Ok(gamesStatistic);
    }

    [HttpGet("packages")]
    public async Task<ActionResult<PackageStatistic>> GetLatestTopPackagesAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken)
    {
        var packagesStatistic = await _gamesService.GetPackagesStatisticAsync(statisticFilter, cancellationToken);

        return Ok(packagesStatistic);
    }

    [HttpPost("reports")]
    public async Task<IActionResult> SendGameReportAsync(GameReport gameReport, CancellationToken cancellationToken = default)
    {
        var gameInfo = gameReport.Info
            ?? throw new ServiceException(WellKnownSIStatisticServiceErrorCode.GameInfoNotFound, System.Net.HttpStatusCode.BadRequest);

        if (gameInfo.Platform != GamePlatforms.Local)
        {
            throw new ServiceException(WellKnownSIStatisticServiceErrorCode.UnsupportedPlatform, System.Net.HttpStatusCode.BadRequest);
        }

        return await ProcessGameReportAsync(gameInfo, cancellationToken);
    }

    [HttpPost("reports/server")]
    public async Task<IActionResult> SendGameServerReportAsync(GameReport gameReport, CancellationToken cancellationToken = default)
    {
        var gameInfo = gameReport.Info
            ?? throw new ServiceException(WellKnownSIStatisticServiceErrorCode.GameInfoNotFound, System.Net.HttpStatusCode.BadRequest);

        return await ProcessGameReportAsync(gameInfo, cancellationToken);
    }

    private async Task<IActionResult> ProcessGameReportAsync(GameResultInfo gameInfo, CancellationToken cancellationToken)
    {
        if (DateTimeOffset.UtcNow.Subtract(gameInfo.FinishTime).TotalHours > 1.0)
        {
            throw new ServiceException(WellKnownSIStatisticServiceErrorCode.InvalidFinishTime, System.Net.HttpStatusCode.BadRequest);
        }

        await _gamesService.AddGameResultAsync(gameInfo, cancellationToken);

        return Accepted();
    }
}