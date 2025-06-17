using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Exceptions;

namespace SIStatisticsService.Controllers;

/// <summary>
/// Provides API for working with games.
/// </summary>
[Route("api/v1/games")]
[ApiController]
public sealed class GamesController(
    IGamesService gamesService,
    IPackagesService packagesService,
    IOptions<SIStatisticsServiceOptions> options) : ControllerBase
{
    private readonly SIStatisticsServiceOptions _options = options.Value;

    [HttpGet("results")]
    public async Task<ActionResult<GamesResponse>> GetLatestGamesInfoAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken = default)
    {
        var games = await gamesService.GetGamesByFilterAsync(statisticFilter, cancellationToken);

        return Ok(new GamesResponse { Results = games });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<GamesStatistic>> GetLatestGamesStatisticAsync(
        [FromQuery] StatisticFilter statisticFilter,
        CancellationToken cancellationToken = default)
    {
        var gamesStatistic = await gamesService.GetGamesStatisticAsync(statisticFilter, cancellationToken);

        return Ok(gamesStatistic);
    }

    [HttpGet("packages")]
    public async Task<ActionResult<PackagesStatistic>> GetLatestTopPackagesAsync(
        [FromQuery] StatisticFilter statisticFilter,
        [FromQuery] Uri? source = null,
        CancellationToken cancellationToken = default)
    {
        var packagesStatistic = await gamesService.GetPackagesStatisticAsync(statisticFilter, source, cancellationToken);

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

        if (gameInfo.Duration < TimeSpan.Zero || gameInfo.Duration > _options.MaximumGameDuration)
        {
            throw new ServiceException(WellKnownSIStatisticServiceErrorCode.InvalidDuration, System.Net.HttpStatusCode.BadRequest);
        }

        await gamesService.AddGameResultAsync(gameInfo, cancellationToken);

        foreach (var questionReport in gameReport.QuestionReports)
        {
            await packagesService.ImportQuestionReportAsync(questionReport, cancellationToken);
        }

        return Accepted();
    }
}
