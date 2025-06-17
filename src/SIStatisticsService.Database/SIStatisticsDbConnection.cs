using LinqToDB;
using LinqToDB.Data;
using SIStatisticsService.Database.Models.Games;
using SIStatisticsService.Database.Models.Questions;

namespace SIStatisticsService.Database;

/// <summary>
/// Defines a database context.
/// </summary>
public sealed class SIStatisticsDbConnection(DataOptions dataOptions) : DataConnection(dataOptions)
{

    /// <summary>
    /// Common entities.
    /// </summary>
    public ITable<EntityModel> Entities => this.GetTable<EntityModel>();

    /// <summary>
    /// Games.
    /// </summary>
    public ITable<GameModel> Games => this.GetTable<GameModel>();

    /// <summary>
    /// Game packages.
    /// </summary>
    public ITable<PackageModel> Packages => this.GetTable<PackageModel>();

    /// <summary>
    /// Package sources.
    /// </summary>
    public ITable<PackageSourceModel> PackageSources => this.GetTable<PackageSourceModel>();

    /// <summary>
    /// Questions.
    /// </summary>
    public ITable<QuestionModel> Questions => this.GetTable<QuestionModel>();

    /// <summary>
    /// Question-theme-entity relations.
    /// </summary>
    public ITable<RelationModel> Relations => this.GetTable<RelationModel>();

    /// <summary>
    /// Themes.
    /// </summary>
    public ITable<ThemeModel> Themes => this.GetTable<ThemeModel>();

    /// <summary>
    /// Game languages.
    /// </summary>
    public ITable<LanguageModel> Languages => this.GetTable<LanguageModel>();
}
