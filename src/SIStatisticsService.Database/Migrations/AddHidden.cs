using FluentMigrator;
using SIStatisticsService.Database.Models.Games;

namespace SIStatisticsService.Database.Migrations;

[Migration(20250531000000, "Add hidden package flag")]
public sealed class AddHidden : Migration
{
    private const string PackagesHiddenIndex = $"IX_{DbConstants.Packages}_Hidden";

    public override void Up()
    {
        Alter.Table(DbConstants.Packages)
            .AddColumn(nameof(PackageModel.Hidden)).AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Index(PackagesHiddenIndex)
            .OnTable(DbConstants.Packages)
            .OnColumn(nameof(PackageModel.Hidden)).Ascending();
    }

    public override void Down()
    {
        Delete.Index(PackagesHiddenIndex).OnTable(DbConstants.Packages);
        Delete.Column(nameof(PackageModel.Hidden)).FromTable(DbConstants.Packages);
    }
}
