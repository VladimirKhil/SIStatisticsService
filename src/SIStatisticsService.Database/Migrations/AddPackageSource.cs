using FluentMigrator;
using SIStatisticsService.Database.Models.Games;

namespace SIStatisticsService.Database.Migrations;

[Migration(20250615000000, "Add PackageSource table")]
public sealed class AddPackageSource : Migration
{
    private const string PackageSourcesConstraintName = $"UC_{DbConstants.PackageSources}";

    public override void Up()
    {
        Create.Table(DbConstants.PackageSources)
            .WithColumn(nameof(PackageSourceModel.PackageId)).AsInt32().NotNullable()
                .ForeignKey(nameof(DbConstants.Packages), nameof(PackageModel.Id))
            .WithColumn(nameof(PackageSourceModel.SourceTypeId)).AsInt32().NotNullable()
            .WithColumn(nameof(PackageSourceModel.Source)).AsString().NotNullable();

        Create.PrimaryKey($"PK_{DbConstants.PackageSources}")
            .OnTable(DbConstants.PackageSources)
            .Columns(nameof(PackageSourceModel.PackageId), nameof(PackageSourceModel.SourceTypeId));
    }

    public override void Down()
    {
        Delete.Table(DbConstants.PackageSources);
    }
}
