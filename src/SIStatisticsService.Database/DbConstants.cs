namespace SIStatisticsService.Database;

/// <summary>
/// Provides well-known SIStatistics database constants.
/// </summary>
public static class DbConstants
{
    public const string DbName = "sistatistics";

    public const string QuestionsSchema = "sistatistics";

    public const string Entities = nameof(Entities);
    public const string Questions = nameof(Questions);
    public const string Themes = nameof(Themes);
    public const string Relations = nameof(Relations);

    public const string GamesSchema = QuestionsSchema; // Could be different in the future

    public const string Packages = nameof(Packages);
    public const string Games = nameof(Games);
    public const string Languages = nameof(Languages);
}
