namespace SIStatisticsService.Database.Models.Questions;

/// <summary>
/// Defines a theme - question - entity relation type.
/// </summary>
public enum RelationType
{
    /// <summary>
    /// Right answer.
    /// </summary>
    Right,

    /// <summary>
    /// Wrong answer.
    /// </summary>
    Wrong,

    /// <summary>
    /// Apellated answer.
    /// </summary>
    Apellated,

    /// <summary>
    /// Automatically accepted answer.
    /// </summary>
    Accepted,

    /// <summary>
    /// Rejected answer.
    /// </summary>
    Rejected,

    /// <summary>
    /// Complained question (meaning that the question or answer is considered incorrect by some person).
    /// </summary>
    Complained,
}
