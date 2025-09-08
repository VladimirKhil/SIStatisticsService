namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents the result of importing a package with collected appellated and rejected answers.
/// </summary>
public sealed class PackageImportResult
{
    /// <summary>
    /// Collection of appellated and rejected answers mapped by question key.
    /// </summary>
    public Dictionary<QuestionKey, List<CollectedAnswer>> CollectedAnswers { get; set; } = new();
}