using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SIStatisticsService.Database;

/// <summary>
/// Provides a <see cref="IServiceCollection" /> extension that allows to register a SIStorage database.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SIStatistics database to service collection.
    /// </summary>
    public static void AddSIStatisticsDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "SIStatistics")
    {
        var dbConnectionString = configuration.GetConnectionString(connectionStringName);

        services.AddLinqToDBContext<SIStatisticsDbConnection>((provider, options) =>
            options.UsePostgreSQL(dbConnectionString).UseDefaultLogging(provider));

        DatabaseExtensions.InitJsonConversion<string[]>();
        DatabaseExtensions.InitJsonConversion<Dictionary<string, int>>();
        DatabaseExtensions.InitJsonConversion<Dictionary<string, string>>();
    }
}
