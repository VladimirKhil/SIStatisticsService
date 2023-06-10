using Microsoft.AspNetCore.Mvc;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Exceptions;
using System.Xml;

namespace SIStatisticsService.Controllers;

/// <summary>
/// Provides Admin-level API for working with games and packages.
/// </summary>
[Route("api/v1/admin")]
[ApiController]
public sealed class AdminController : ControllerBase
{
    private readonly IGamesService _gamesService;
    private readonly IPackagesService _packagesService;

    public AdminController(IGamesService gamesService, IPackagesService packagesService)
    {
        _gamesService = gamesService;
        _packagesService = packagesService;
    }

    [HttpGet("questions")]
    public async Task<ActionResult<QuestionInfoResponse>> GetQuestionInfoAsync(
        string themeName,
        string questionText,
        CancellationToken cancellationToken)
    {
        var questionInfo = await _packagesService.GetQuestionInfoAsync(themeName, questionText, cancellationToken);

        return Ok(questionInfo);
    }

    [HttpPost("packages")]
    public async Task<IActionResult> ImportPackageContentsAsync()
    {
        var cancellationToken = HttpContext.RequestAborted;

        if (Request.Form.Files.Count == 0)
        {
            throw new ServiceException(WellKnownSIStatisticServiceErrorCode.PackageFileNotFound, System.Net.HttpStatusCode.BadRequest);
        }

        var file = Request.Form.Files[0]
            ?? throw new ServiceException(WellKnownSIStatisticServiceErrorCode.PackageFileNotFound, System.Net.HttpStatusCode.BadRequest);
        
        var package = new SIPackages.Package();

        using (var stream = file.OpenReadStream())
        using (var reader = XmlReader.Create(stream))
        {
            package.ReadXml(reader);
        }

        await _packagesService.ImportPackageAsync(package, cancellationToken);

        return Accepted();
    }

    [HttpPost("reports")]
    public async Task<IActionResult> SendGameReportAsync(GameReport gameReport, CancellationToken cancellationToken = default)
    {
        var gameInfo = gameReport.Info
            ?? throw new ServiceException(WellKnownSIStatisticServiceErrorCode.GameInfoNotFound, System.Net.HttpStatusCode.BadRequest);

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
