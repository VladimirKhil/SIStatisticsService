using Microsoft.AspNetCore.Mvc;
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
    private readonly IPackagesService _packagesService;

    public GamesController(IGamesService gamesService, IPackagesService packagesService)
    {
        _gamesService = gamesService;
        _packagesService = packagesService;
    }

    [HttpGet("results")]
    public async Task<ActionResult<GamesResponse>> GetLatestGamesInfoAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken = default)
    {
        var games = await _gamesService.GetGamesByFilterAsync(statisticFilter, cancellationToken);

        return Ok(new GamesResponse { Results = games });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<GamesStatistic>> GetLatestGamesStatisticAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken = default)
    {
        var gamesStatistic = await _gamesService.GetGamesStatisticAsync(statisticFilter, cancellationToken);

        return Ok(gamesStatistic);
    }

    [HttpGet("packages")]
    public async Task<ActionResult<PackagesStatistic>> GetLatestTopPackagesAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken = default)
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

        if (DateTimeOffset.UtcNow.Subtract(gameInfo.FinishTime).TotalHours > 1.0)
        {
            throw new ServiceException(WellKnownSIStatisticServiceErrorCode.InvalidFinishTime, System.Net.HttpStatusCode.BadRequest);
        }

        await _gamesService.AddGameResultAsync(gameInfo, cancellationToken);

        foreach (var questionReport in gameReport.QuestionReports)
        {
            await _packagesService.ImportQuestionReportAsync(questionReport, cancellationToken);
        }

        return Accepted();
    }
}
