using FluentMigrator;
using SIStatisticsService.Database.Models.Games;
using SIStatisticsService.Database.Models.Questions;

namespace SIStatisticsService.Database.Migrations;

[Migration(202302030000, "Initial migration")]
public sealed class Initial : Migration
{
    public override void Up()
    {
        Create.Table(DbConstants.Packages)
            .WithColumn(nameof(PackageModel.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(PackageModel.Name)).AsString().NotNullable()
            .WithColumn(nameof(PackageModel.Hash)).AsString().NotNullable()
            .WithColumn(nameof(PackageModel.Authors)).AsJsonb().NotNullable();

        Create.Table(DbConstants.Games)
            .WithColumn(nameof(GameModel.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(GameModel.Name)).AsString().NotNullable()
            .WithColumn(nameof(GameModel.Platform)).AsInt16().NotNullable()
            .WithColumn(nameof(GameModel.FinishTime)).AsDateTimeOffset().NotNullable()
            .WithColumn(nameof(GameModel.Duration)).AsInterval().Nullable()
            .WithColumn(nameof(GameModel.Scores)).AsJsonb().NotNullable()
            .WithColumn(nameof(GameModel.Reviews)).AsJsonb().NotNullable()
            .WithColumn(nameof(GameModel.PackageId)).AsInt32().Nullable()
                .ForeignKey(nameof(DbConstants.Packages), nameof(PackageModel.Id));

        Create.Table(DbConstants.Questions)
            .WithColumn(nameof(QuestionModel.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(QuestionModel.Text)).AsString().NotNullable();

        Create.Table(DbConstants.Themes)
            .WithColumn(nameof(ThemeModel.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(ThemeModel.Name)).AsString().NotNullable();

        Create.Table(DbConstants.Entities)
            .WithColumn(nameof(EntityModel.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(EntityModel.Name)).AsString().NotNullable().Unique();

        Create.Table(DbConstants.Relations)
            .WithColumn(nameof(RelationModel.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(RelationModel.QuestionId)).AsInt32().NotNullable()
                .ForeignKey(nameof(DbConstants.Questions), nameof(QuestionModel.Id))
            .WithColumn(nameof(RelationModel.ThemeId)).AsInt32().NotNullable()
                .ForeignKey(nameof(DbConstants.Themes), nameof(ThemeModel.Id))
            .WithColumn(nameof(RelationModel.EntityId)).AsInt32().NotNullable()
                .ForeignKey(nameof(DbConstants.Entities), nameof(EntityModel.Id))
            .WithColumn(nameof(RelationModel.Type)).AsInt16().NotNullable()
            .WithColumn(nameof(RelationModel.Count)).AsInt32().NotNullable();
    }

    public override void Down()
    {
        Delete.Table(DbConstants.Relations);
        Delete.Table(DbConstants.Entities);
        Delete.Table(DbConstants.Themes);
        Delete.Table(DbConstants.Questions);
        Delete.Table(DbConstants.Games);
        Delete.Table(DbConstants.Packages);
    }
}
