using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIStatisticsService.Client;
using SIStatisticsService.Contract;

namespace SIStatisticsService.IntegrationTests;

public abstract class TestsBase
{
    protected ISIStatisticsServiceClient SIStatisticsClient { get; private set; }

    public TestsBase()
    {
        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        var configuration = builder.Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSIStatisticsServiceClient(configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        SIStatisticsClient = serviceProvider.GetRequiredService<ISIStatisticsServiceClient>();
    }
}