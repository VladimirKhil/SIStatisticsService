using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using SIStatisticsService.Metrics;
using SIStatisticsService.Services;

namespace SIStatisticsService.ComponentTests;

internal abstract class TestsBase
{
    protected IPackagesService PackagesService { get; }

    protected IGamesService GamesService { get; }

    public TestsBase()
    {
        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        var configuration = builder.Build();

        var services = new ServiceCollection();

        services.Configure<SIStatisticsServiceOptions>(configuration.GetSection(SIStatisticsServiceOptions.ConfigurationSectionName));

        services.AddSIStatisticsDatabase(configuration);

        var meters = new OtelMetrics();
        services.AddSingleton(meters);

        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton<ILogger<PackagesService>, NullLogger<PackagesService>>();
        services.AddTransient<IGamesService, GamesService>();
        services.AddTransient<IPackagesService, PackagesService>();

        var serviceProvider = services.BuildServiceProvider();

        PackagesService = serviceProvider.GetRequiredService<IPackagesService>();
        GamesService = serviceProvider.GetRequiredService<IGamesService>();
    }
}
