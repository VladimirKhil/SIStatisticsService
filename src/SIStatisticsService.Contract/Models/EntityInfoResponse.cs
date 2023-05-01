namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines an entity info response.
/// </summary>
public sealed class EntityInfoResponse
{
    /// <summary>
    /// Entity name.
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Type of relation with question.
    /// </summary>
    public RelationType RelationType { get; set; }

    /// <summary>
    /// Occurence count.
    /// </summary>
    public int Count { get; set; }
}
