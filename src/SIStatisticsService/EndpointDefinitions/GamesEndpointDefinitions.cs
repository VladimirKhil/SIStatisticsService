using Microsoft.Extensions.Options;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Exceptions;

namespace SIStatisticsService.EndpointDefinitions;

internal static class GamesEndpointDefinitions
{
    public static void DefineGamesEndpoint(this WebApplication app)
    {
        var gamesGroup = app.MapGroup("/api/v1/games");

        // GET /api/v1/games/results
        gamesGroup.MapGet("/results", async (
            IGamesService gamesService,
            [AsParameters] StatisticFilter statisticFilter,
            CancellationToken cancellationToken = default) =>
        {
            var games = await gamesService.GetGamesByFilterAsync(statisticFilter, cancellationToken);
            return Results.Ok(new GamesResponse { Results = games });
        });

        // GET /api/v1/games/stats
        gamesGroup.MapGet("/stats", async (
            IGamesService gamesService,
            [AsParameters] StatisticFilter statisticFilter,
            CancellationToken cancellationToken = default) =>
        {
            var gamesStatistic = await gamesService.GetGamesStatisticAsync(statisticFilter, cancellationToken);
            return Results.Ok(gamesStatistic);
        });

        // GET /api/v1/games/packages
        gamesGroup.MapGet("/packages", async (
            IGamesService gamesService,
            [AsParameters] StatisticFilter statisticFilter,
            Uri? source = null,
            Uri? fallbackSource = null,
            CancellationToken cancellationToken = default) =>
        {
            var packagesStatistic = await gamesService.GetPackagesStatisticAsync(new TopPackagesRequest(statisticFilter, source, fallbackSource), cancellationToken);
            return Results.Ok(packagesStatistic);
        });

        // POST /api/v1/games/reports
        gamesGroup.MapPost("/reports", async (
            IGamesService gamesService,
            IPackagesService packagesService,
            IOptions<SIStatisticsServiceOptions> options,
            GameReport gameReport,
            CancellationToken cancellationToken = default) =>
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

            if (gameInfo.Duration < TimeSpan.Zero || gameInfo.Duration > options.Value.MaximumGameDuration)
            {
                throw new ServiceException(WellKnownSIStatisticServiceErrorCode.InvalidDuration, System.Net.HttpStatusCode.BadRequest);
            }

            await gamesService.AddGameResultAsync(gameInfo, cancellationToken);

            foreach (var questionReport in gameReport.QuestionReports)
            {
                await packagesService.ImportQuestionReportAsync(questionReport, cancellationToken);
            }

            return Results.Accepted();
        });
    }
}
