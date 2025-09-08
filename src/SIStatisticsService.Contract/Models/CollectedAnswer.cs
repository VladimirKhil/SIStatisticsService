namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents a collected answer with its details.
/// </summary>
public sealed class CollectedAnswer
{
    /// <summary>
    /// Answer text.
    /// </summary>
    public string? AnswerText { get; set; }

    /// <summary>
    /// Relation type (Apellated or Rejected).
    /// </summary>
    public RelationType RelationType { get; set; }

    /// <summary>
    /// Number of occurrences.
    /// </summary>
    public int Count { get; set; }
}