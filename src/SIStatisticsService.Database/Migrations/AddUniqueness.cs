using FluentMigrator;
using SIStatisticsService.Database.Models.Questions;

namespace SIStatisticsService.Database.Migrations;

[Migration(202307080000, "Add uniqueness")]
public sealed class AddUniqueness : Migration
{
    public override void Up()
    {
        Alter.Table(DbConstants.Questions)
            .AlterColumn(nameof(QuestionModel.Text)).AsString().Unique().NotNullable();

        Alter.Table(DbConstants.Themes)
            .AlterColumn(nameof(ThemeModel.Name)).AsString().Unique().NotNullable();
    }

    public override void Down()
    {
        Alter.Table(DbConstants.Themes)
            .AlterColumn(nameof(ThemeModel.Name)).AsString().NotNullable();

        Alter.Table(DbConstants.Questions)
    .       AlterColumn(nameof(QuestionModel.Text)).AsString().NotNullable();
    }
}
