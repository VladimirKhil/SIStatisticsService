using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Exceptions;
using System.Xml;

namespace SIStatisticsService.EndpointDefinitions;

internal static class AdminEndpointDefinitions
{
    public static void DefineAdminEndpoint(this WebApplication app)
    {
        var adminGroup = app.MapGroup("/api/v1/admin");

        // GET /api/v1/admin/questions
        adminGroup.MapGet("/questions", async (
            IPackagesService packagesService,
            string themeName,
            string questionText,
            CancellationToken cancellationToken = default) =>
        {
            var questionInfo = await packagesService.GetQuestionInfoAsync(themeName, questionText, cancellationToken);
            return Results.Ok(questionInfo);
        });

        // POST /api/v1/admin/packages
        adminGroup.MapPost("/packages", async (
            IPackagesService packagesService,
            HttpContext context) =>
        {
            var cancellationToken = context.RequestAborted;

            if (context.Request.Form.Files.Count == 0)
            {
                throw new ServiceException(WellKnownSIStatisticServiceErrorCode.PackageFileNotFound, System.Net.HttpStatusCode.BadRequest);
            }

            var file = context.Request.Form.Files[0]
                ?? throw new ServiceException(WellKnownSIStatisticServiceErrorCode.PackageFileNotFound, System.Net.HttpStatusCode.BadRequest);
            
            var package = new SIPackages.Package();

            using (var stream = file.OpenReadStream())
            using (var reader = XmlReader.Create(stream))
            {
                package.ReadXml(reader);
            }

            var result = await packagesService.ImportPackageAsync(package, cancellationToken);

            return Results.Ok(result);
        });

        // POST /api/v1/admin/reports
        adminGroup.MapPost("/reports", async (
            IGamesService gamesService,
            IPackagesService packagesService,
            GameReport gameReport,
            CancellationToken cancellationToken = default) =>
        {
            var gameInfo = gameReport.Info
                ?? throw new ServiceException(WellKnownSIStatisticServiceErrorCode.GameInfoNotFound, System.Net.HttpStatusCode.BadRequest);

            if (gameInfo.Package.Hash == null)
            {
                throw new ServiceException(WellKnownSIStatisticServiceErrorCode.MissingPackageHash, System.Net.HttpStatusCode.BadRequest);
            }

            if (DateTimeOffset.UtcNow.Subtract(gameInfo.FinishTime).TotalHours > 1.0)
            {
                throw new ServiceException(WellKnownSIStatisticServiceErrorCode.InvalidFinishTime, System.Net.HttpStatusCode.BadRequest);
            }

            if (gameInfo.Platform != GamePlatforms.Local && gameInfo.Platform != GamePlatforms.GameServer)
            {
                throw new ServiceException(WellKnownSIStatisticServiceErrorCode.UnsupportedPlatform, System.Net.HttpStatusCode.BadRequest);
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
