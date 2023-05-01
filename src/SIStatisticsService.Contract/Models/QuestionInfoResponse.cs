namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Defines question info.
/// </summary>
public sealed class QuestionInfoResponse
{
    /// <summary>
    /// Question related entities.
    /// </summary>
    public EntityInfoResponse[] Entities { get; set; } = Array.Empty<EntityInfoResponse>();
}
