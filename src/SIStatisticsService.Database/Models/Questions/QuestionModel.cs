using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Questions;

/// <summary>
/// Represents a question.
/// </summary>
[Table(Schema = DbConstants.QuestionsSchema, Name = DbConstants.Questions)]
public sealed class QuestionModel
{
    /// <summary>
    /// Question identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull, Identity]
    public int Id { get; set; }

    /// <summary>
    /// Question text.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string? Text { get; set; }
}
