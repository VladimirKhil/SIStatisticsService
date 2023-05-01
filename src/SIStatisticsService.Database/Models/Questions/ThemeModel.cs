using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Questions;

/// <summary>
/// Represents a theme.
/// </summary>
[Table(Schema = DbConstants.QuestionsSchema, Name = DbConstants.Themes)]
public sealed class ThemeModel
{
    /// <summary>
    /// Theme identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull, Identity]
    public int Id { get; set; }

    /// <summary>
    /// Theme name.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string? Name { get; set; }
}
