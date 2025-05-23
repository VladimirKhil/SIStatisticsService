using AspNetCoreRateLimit;
using EnsureThat;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using SIStatisticsService.Metrics;
using SIStatisticsService.Middlewares;
using SIStatisticsService.Services;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(new Serilog.Formatting.Display.MessageTemplateTextFormatter(
        "[{Timestamp:yyyy/MM/dd HH:mm:ss} {Level}] {Message:lj} {Exception}{NewLine}"))
    .WriteTo.OpenTelemetry(options => options.ResourceAttributes = new Dictionary<string, object>
    {
        ["service.name"] = "SIStatistics"
    })
    .ReadFrom.Configuration(ctx.Configuration)
    .Filter.ByExcluding(logEvent =>
        logEvent.Exception is PostgresException pe && (logEvent.MessageTemplate.Text.Contains("index row size") || pe.SqlState == PostgresErrorCodes.QueryCanceled)
        || logEvent.Exception is BadHttpRequestException
        || logEvent.Exception is OperationCanceledException));

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();

Configure(app);

app.Run();

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<SIStatisticsServiceOptions>(configuration.GetSection(SIStatisticsServiceOptions.ConfigurationSectionName));

    services.AddControllers();

    services.AddSIStatisticsDatabase(configuration);
    ConfigureMigrationRunner(services, configuration);

    services.AddTransient<IGamesService, GamesService>();
    services.AddTransient<IPackagesService, PackagesService>();

    AddRateLimits(services, configuration);
    AddMetrics(services);
}

static void ConfigureMigrationRunner(IServiceCollection services, IConfiguration configuration)
{
    services.AddSingleton<IConventionSet>(new DefaultConventionSet(DbConstants.QuestionsSchema, null));

    var dbConnectionString = configuration.GetConnectionString("SIStatistics");

    services
        .AddFluentMigratorCore()
        .ConfigureRunner(migratorBuilder =>
            migratorBuilder
                .AddPostgres()
                .WithGlobalConnectionString(dbConnectionString)
                .ScanIn(typeof(DbConstants).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole());
}

static void Configure(WebApplication app)
{
    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.UseRouting();
    app.MapControllers();

    CreateDatabase(app);
    ApplyMigrations(app);

    app.UseIpRateLimiting();
}

static void AddRateLimits(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimit"));

    services.AddMemoryCache();
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    services.AddInMemoryRateLimiting();
}

static void AddMetrics(IServiceCollection services)
{
    services.AddSingleton<OtelMetrics>();

    services.AddOpenTelemetry().WithMetrics(builder =>
        builder
            .ConfigureResource(rb => rb.AddService("SIStatistics"))
            .AddMeter(OtelMetrics.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddOtlpExporter());
}

static void CreateDatabase(WebApplication app)
{
    var dbConnectionString = app.Configuration.GetConnectionString("SIStatistics");

    Ensure.That(dbConnectionString).IsNotNullOrEmpty();

    var connectionStringBuilder = new DbConnectionStringBuilder
    {
        ConnectionString = dbConnectionString
    };

    connectionStringBuilder["Database"] = "postgres";

    DatabaseExtensions.EnsureExists(connectionStringBuilder.ConnectionString!, DbConstants.DbName);
}

static void ApplyMigrations(WebApplication app)
{
    var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

    if (runner.HasMigrationsToApplyUp())
    {
        runner.MigrateUp();
    }
}