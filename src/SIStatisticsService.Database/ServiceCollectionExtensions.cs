﻿using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Data.RetryPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace SIStatisticsService.Database;

/// <summary>
/// Provides a <see cref="IServiceCollection" /> extension that allows to register a SIStorage database.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SIStatistics database to service collection.
    /// </summary>
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(SIStatisticsDbConnection))]
    public static void AddSIStatisticsDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "SIStatistics")
    {
        var dbConnectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException("Database connection is undefined");

        services.AddLinqToDBContext<SIStatisticsDbConnection>((provider, options) =>
            options
                .UsePostgreSQL(dbConnectionString)
                .UseRetryPolicy(new TransientRetryPolicy())
                .UseDefaultLogging(provider));

        DatabaseExtensions.InitJsonConversion<string[]>();
        DatabaseExtensions.InitJsonConversion<Dictionary<string, int>>();
        DatabaseExtensions.InitJsonConversion<Dictionary<string, string>>();
    }
}
