using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Games;

/// <summary>
/// Defines a game package.
/// </summary>
[Table(Schema = DbConstants.GamesSchema, Name = DbConstants.Packages)]
public sealed class PackageModel
{
    /// <summary>
    /// Package identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull, Identity]
    public int Id { get; set; }

    /// <summary>
    /// Package name.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// Package hash.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string? Hash { get; set; }

    /// <summary>
    /// Package authors.
    /// </summary>
    [Column(DataType = DataType.BinaryJson), NotNull]
    public string[] Authors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Package author contacts.
    /// </summary>
    [Column(DataType = DataType.NVarChar), Nullable]
    public string? AuthorsContacts { get; set; }
}
