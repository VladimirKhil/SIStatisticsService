using Microsoft.AspNetCore.Mvc;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Exceptions;
using System.Xml;

namespace SIStatisticsService.Controllers;

/// <summary>
/// Provides API for working with packages.
/// </summary>
[Route("api/v1/packages")]
[ApiController]
public sealed class PackagesController : ControllerBase
{
    private readonly IPackagesService _packagesService;

    public PackagesController(IPackagesService packagesService) => _packagesService = packagesService;

    [HttpGet("questions")]
    public async Task<ActionResult<QuestionInfoResponse>> GetQuestionInfoAsync(
        string themeName,
        string questionText,
        CancellationToken cancellationToken)
    {
        var questionInfo = await _packagesService.GetQuestionInfoAsync(themeName, questionText, cancellationToken);

        return Ok(questionInfo);
    }

    [HttpPost]
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
}
