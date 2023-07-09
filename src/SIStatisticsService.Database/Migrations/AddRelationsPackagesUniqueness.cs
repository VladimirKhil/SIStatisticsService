using FluentMigrator;
using SIStatisticsService.Database.Models.Games;
using SIStatisticsService.Database.Models.Questions;

namespace SIStatisticsService.Database.Migrations;

[Migration(202307090000, "Add relations and packages uniqueness")]
public sealed class AddRelationsPackagesUniqueness : Migration
{
    private const string RelationsConstraintName = $"UC_{DbConstants.Relations}";
    private const string PackagesConstraintName = $"UC_{DbConstants.Packages}";

    public override void Up()
    {
        Create.UniqueConstraint(RelationsConstraintName)
            .OnTable(DbConstants.Relations)
            .Columns(nameof(RelationModel.ThemeId), nameof(RelationModel.QuestionId), nameof(RelationModel.EntityId), nameof(RelationModel.Type));

        Create.UniqueConstraint(PackagesConstraintName)
            .OnTable(DbConstants.Packages)
            .Columns(nameof(PackageModel.Name), nameof(PackageModel.Hash), nameof(PackageModel.Authors));
    }

    public override void Down()
    {
        Delete.UniqueConstraint(PackagesConstraintName).FromTable(DbConstants.Packages);
        Delete.UniqueConstraint(RelationsConstraintName).FromTable(DbConstants.Relations);
    }
}
