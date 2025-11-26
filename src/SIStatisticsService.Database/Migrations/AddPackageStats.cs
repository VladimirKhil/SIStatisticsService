using FluentMigrator;
using SIStatisticsService.Database.Models.Games;

namespace SIStatisticsService.Database.Migrations;

[Migration(20250616000000, "Add Stats field to Packages table")]
public sealed class AddPackageStats : Migration
{
    public override void Up() => Alter.Table(DbConstants.Packages).AddColumn(nameof(PackageModel.Stats)).AsJsonb().Nullable();

    public override void Down() => Delete.Column(nameof(PackageModel.Stats)).FromTable(DbConstants.Packages);
}