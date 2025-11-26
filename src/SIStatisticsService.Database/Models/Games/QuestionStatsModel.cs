namespace SIStatisticsService.Database.Models.Games;

/// <summary>
/// Defines statistics for a question.
/// </summary>
/// <param name="ShownCount">Number of times question was shown.</param>
/// <param name="PlayerSeenCount">Number of players that have seen the question.</param>
/// <param name="CorrectCount">Number of correct answers for the question.</param>
/// <param name="WrongCount">Number of wrong answers for the question.</param>
public sealed record QuestionStatsModel(
    int ShownCount,
    int PlayerSeenCount,
    int CorrectCount,
    int WrongCount);
