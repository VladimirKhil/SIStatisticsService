using FluentMigrator;
using SIStatisticsService.Database.Models.Games;

namespace SIStatisticsService.Database.Migrations;

[Migration(202401120000, "Add authors contacts and language")]
public sealed class AddAuthorsContactsAndLanguage : Migration
{
    private const string GamesStatisticsIndex = $"IX_{DbConstants.Games}_Statistics";

    public override void Up()
    {
        Alter.Table(DbConstants.Packages).AddColumn(nameof(PackageModel.AuthorsContacts)).AsString().Nullable();

        Create.Table(DbConstants.Languages)
            .WithColumn(nameof(LanguageModel.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(LanguageModel.Code)).AsString(16).NotNullable().Unique();

        Insert.IntoTable(DbConstants.Languages).Row(new { Code = "en" });
        Insert.IntoTable(DbConstants.Languages).Row(new { Code = "ru" });

        Alter.Table(DbConstants.Games)
            .AddColumn(nameof(GameModel.LanguageId))
            .AsInt32()
            .Nullable()
            .ForeignKey(nameof(DbConstants.Languages), nameof(LanguageModel.Id));

        Create.Index(GamesStatisticsIndex).OnTable(DbConstants.Games)
            .OnColumn(nameof(GameModel.FinishTime)).Descending()
            .OnColumn(nameof(GameModel.Platform)).Ascending()
            .OnColumn(nameof(GameModel.LanguageId)).Ascending();
    }
    public override void Down()
    {
        Delete.Index(GamesStatisticsIndex).OnTable(DbConstants.Games);
        Delete.Column(nameof(GameModel.LanguageId)).FromTable(DbConstants.Games);
        Delete.Table(DbConstants.Languages);
        Delete.Column(nameof(PackageModel.AuthorsContacts)).FromTable(DbConstants.Packages);
    }
}
