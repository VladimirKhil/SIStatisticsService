using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Games;

/// <summary>
/// Represents a game model.
/// </summary>
[Table(Schema = DbConstants.GamesSchema, Name = DbConstants.Games)]
public sealed class GameModel
{
    /// <summary>
    /// Entity identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull, Identity]
    public int Id { get; set; }

    /// <summary>
    /// Game language identifier.
    /// </summary>
    [Column(DataType = DataType.Int32), Nullable]
    public int? LanguageId { get; set; }

    /// <summary>
    /// Game name.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string Name { get; set; } = "";

    /// <summary>
    /// Game platform.
    /// </summary>
    [Column(DataType = DataType.Int32), NotNull]
    public GamePlatform Platform { get; set; }

    /// <summary>
    /// Game finish time.
    /// </summary>
    [Column(DataType = DataType.DateTimeOffset), NotNull]
    public DateTimeOffset FinishTime { get; set; }

    /// <summary>
    /// Game duration.
    /// </summary>
    [Column(DataType = DataType.Interval), NotNull]
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Game package info.
    /// </summary>
    [Column(DataType = DataType.Int32), Nullable]
    public int? PackageId { get; set; }

    /// <summary>
    /// Game scores.
    /// </summary>
    [Column(DataType = DataType.BinaryJson), Nullable]
    public Dictionary<string, int> Scores { get; set; } = new();

    /// <summary>
    /// Game reviews.
    /// </summary>
    [Column(DataType = DataType.BinaryJson), Nullable]
    public Dictionary<string, string> Reviews { get; set; } = new();
}
