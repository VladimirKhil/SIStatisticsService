using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using SIStatisticsService.Metrics;
using SIStatisticsService.Services;
using Testcontainers.PostgreSql;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;

namespace SIStatisticsService.ComponentTests;

/// <summary>
/// Defines base class for component tests that provides PostgreSQL TestContainer setup.
/// </summary>
internal abstract class TestContainerBase
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

	private IServiceProvider _serviceProvider = null!;

    protected IPackagesService PackagesService { get; private set; } = null!;

	protected IGamesService GamesService { get; private set; } = null!;

    protected TestContainerBase()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("sistatistics")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
    }

	/// <summary>
    /// Initializes the test container and sets up the database
    /// </summary>
    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        // Start the PostgreSQL container
        await _postgreSqlContainer.StartAsync();

        // Set up services with the container connection string
        SetupServices();

        // Apply database migrations
        await ApplyMigrationsAsync();
    }

    /// <summary>
    /// Cleans up the test container
    /// </summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        await _postgreSqlContainer.DisposeAsync();
    }

    private void SetupServices()
    {
        var connectionString = _postgreSqlContainer.GetConnectionString();

        // Create configuration with the test container connection string
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SIStatistics"] = connectionString
            })
            .Build();

        var services = new ServiceCollection();

        // Configure services
        services.Configure<SIStatisticsServiceOptions>(options =>
        {
            options.TopPackageCount = 10;
            options.MaxResultCount = 100;
            options.MaximumGameDuration = TimeSpan.FromHours(10);
        });

        // Add database services
        services.AddSIStatisticsDatabase(configuration);

        // Add migration services
        services.AddSingleton<IConventionSet>(new DefaultConventionSet(DbConstants.QuestionsSchema, null));

		services.AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(DbConstants).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        // Add metrics
        services.AddMetrics();
        services.AddSingleton<OtelMetrics>();

        // Add logging
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton<ILogger<PackagesService>, NullLogger<PackagesService>>();
        services.AddSingleton<ILogger<GamesService>, NullLogger<GamesService>>();

        // Add business services
        services.AddTransient<IGamesService, GamesService>();
        services.AddTransient<IPackagesService, PackagesService>();

        _serviceProvider = services.BuildServiceProvider();

        // Resolve services
        PackagesService = _serviceProvider.GetRequiredService<IPackagesService>();
        GamesService = _serviceProvider.GetRequiredService<IGamesService>();
    }

    private async Task ApplyMigrationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        // Apply all migrations
        await Task.Run(() =>
        {
            if (runner.HasMigrationsToApplyUp())
            {
                runner.MigrateUp();
            }
        });
    }
}
