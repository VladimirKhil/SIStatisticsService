namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines a game question report.
/// </summary>
public sealed class QuestionReport
{
    /// <summary>
    /// Theme name.
    /// </summary>
    public string? ThemeName { get; set; }

    /// <summary>
    /// Question text.
    /// </summary>
    public string? QuestionText { get; set; }

    /// <summary>
    /// Report type.
    /// </summary>
    public QuestionReportType ReportType { get; set; }

    /// <summary>
    /// Report text (complain text when <see cref="ReportType" /> equals <see cref="QuestionReportType.Complained" />
    /// and question answer for other report types).
    /// </summary>
    public string? ReportText { get; set; }
}
