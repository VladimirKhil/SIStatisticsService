using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using SIStatisticsService.Contract;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SIStatisticsService.Client;

/// <summary>
/// Provides an extension method for adding <see cref="ISIStatisticsServiceClient" /> implementation to service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ISIStatisticsServiceClient" /> implementation to service collection.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">App configuration.</param>
    public static IServiceCollection AddSIStatisticsServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        var optionsSection = configuration.GetSection(SIStatisticsClientOptions.ConfigurationSectionName);
        services.Configure<SIStatisticsClientOptions>(optionsSection);

        var options = optionsSection.Get<SIStatisticsClientOptions>();

        services.AddHttpClient<ISIStatisticsServiceClient, SIStatisticsServiceClient>(
            client =>
            {
                var serviceUri = options?.ServiceUri;
                client.BaseAddress = serviceUri != null ? new Uri(serviceUri, "api/v1/") : null;
                client.DefaultRequestVersion = HttpVersion.Version20;

                if (options != null)
                {
                    SetAuthSecret(options, client);
                }
            }).AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    options?.RetryCount ?? SIStatisticsClientOptions.DefaultRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt))));

        return services;
    }

    private static void SetAuthSecret(SIStatisticsClientOptions options, HttpClient client)
    {
        if (options.ClientSecret == null)
        {
            return;
        }

        var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"admin:{options.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
    }
}
