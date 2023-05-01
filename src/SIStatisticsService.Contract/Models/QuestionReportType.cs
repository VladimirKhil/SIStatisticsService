namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents a question report type.
/// </summary>
public enum QuestionReportType
{
    /// <summary>
    /// Apellated answer.
    /// </summary>
    Apellated,

    /// <summary>
    /// Automatically accepted answer.
    /// </summary>
    Accepted,

    /// <summary>
    /// Rejected (wrong) answer.
    /// </summary>
    Rejected,

    /// <summary>
    /// Complained question (meaning that the question or answer is considered incorrect by some person).
    /// </summary>
    Complained,
}
