namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines a game report.
/// </summary>
public sealed class GameReport
{
    /// <summary>
    /// Unique report identifier.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Game info.
    /// </summary>
    public GameResultInfo? Info { get; set; }

    /// <summary>
    /// Question reports.
    /// </summary>
    public QuestionReport[] QuestionReports { get; set; } = Array.Empty<QuestionReport>();
}
