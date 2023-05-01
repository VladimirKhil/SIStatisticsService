using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Questions;

/// <summary>
/// Represents a common entity.
/// </summary>
[Table(Schema = DbConstants.GamesSchema, Name = DbConstants.Entities)]
public sealed class EntityModel
{
    /// <summary>
    /// Entity identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull, Identity]
    public int Id { get; set; }

    /// <summary>
    /// Entity name.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string? Name { get; set; }
}
