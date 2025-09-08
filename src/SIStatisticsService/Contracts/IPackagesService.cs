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
    Task<QuestionInfoResponse> GetQuestionInfoAsync(string themeName, string questionText, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Imports package data and collects appellated and rejected answers.
    /// </summary>
    /// <param name="package">Package data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Import result with collected appellated and rejected answers.</returns>
    Task<PackageImportResult> ImportPackageAsync(Package package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports question report.
    /// </summary>
    /// <param name="questionReport">Question report.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ImportQuestionReportAsync(QuestionReport questionReport, CancellationToken cancellationToken = default);
}
