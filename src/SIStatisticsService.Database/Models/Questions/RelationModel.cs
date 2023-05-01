using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Questions;

/// <summary>
/// Defines a question-theme-entity relation.
/// </summary>
[Table(Schema = DbConstants.QuestionsSchema, Name = DbConstants.Relations)]
public sealed class RelationModel
{
    /// <summary>
    /// Question-theme-entity identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull, Identity]
    public int Id { get; set; }

    /// <summary>
    /// Question identifier.
    /// </summary>
    [Column(DataType = DataType.Int32), NotNull]
    public int QuestionId { get; set; }

    /// <summary>
    /// Theme identifier.
    /// </summary>
    [Column(DataType = DataType.Int32), NotNull]
    public int ThemeId { get; set; }

    /// <summary>
    /// Entity identifier.
    /// </summary>
    [Column(DataType = DataType.Int32), NotNull]
    public int EntityId { get; set; }

    /// <summary>
    /// Relation type.
    /// </summary>
    [Column(DataType = DataType.Int32), NotNull]
    public RelationType Type { get; set; }

    /// <summary>
    /// Relation counter.
    /// </summary>
    [Column(DataType = DataType.Int32), NotNull]
    public int Count { get; set; }
}
