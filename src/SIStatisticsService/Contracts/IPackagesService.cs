using SIPackages;
using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.Contracts;

/// <summary>
/// Provides API for working with package data.
/// </summary>
public interface IPackagesService
{
    /// <summary>
    /// Gets question semantic information.
    /// </summary>
    /// <param name="themeName">Question theme name.</param>
    /// <param name="questionText">Question text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<QuestionInfoResponse> GetQuestionInfoAsync(string themeName, string questionText, CancellationToken cancellationToken);
    
    /// <summary>
    /// Imports package data.
    /// </summary>
    /// <param name="package">Package data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ImportPackageAsync(Package package, CancellationToken cancellationToken);

    /// <summary>
    /// Imports question report.
    /// </summary>
    /// <param name="questionReport">Question report.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ImportQuestionReportAsync(QuestionReport questionReport, CancellationToken cancellationToken);
}
