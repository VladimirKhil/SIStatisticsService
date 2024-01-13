using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Games;

/// <summary>
/// Provides a language info.
/// </summary>
[Table(Schema = DbConstants.GamesSchema, Name = DbConstants.Languages)]
public sealed class LanguageModel
{
    /// <summary>
    /// Language identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull, Identity]
    public int Id { get; set; }

    /// <summary>
    /// Language code.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string? Code { get; set; }
}
