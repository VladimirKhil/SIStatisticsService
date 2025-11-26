using LinqToDB;
using LinqToDB.Mapping;

namespace SIStatisticsService.Database.Models.Games;

/// <summary>
/// Defines a package source, which is a URI where the package can be found.
/// </summary>
[Table(Schema = DbConstants.GamesSchema, Name = DbConstants.PackageSources)]
public sealed class PackageSourceModel
{
    /// <summary>
    /// Package identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull]
    public int PackageId { get; set; }

    /// <summary>
    /// Source type identifier.
    /// </summary>
    [PrimaryKey, Column(DataType = DataType.Int32), NotNull]
    public int SourceTypeId { get; set; }

    /// <summary>
    /// Package source URI.
    /// </summary>
    [Column(DataType = DataType.NVarChar), NotNull]
    public string Source { get; set; } = "";
}
