using Microsoft.Extensions.Options;
using SIStatisticsService.Contract;
using SIStatisticsService.Contract.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SIStatisticsService.Client;

/// <inheritdoc cref="ISIStatisticsServiceClient" />
internal sealed class SIStatisticsServiceClient : ISIStatisticsServiceClient
{
    private const int BufferSize = 80 * 1024;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new QuestionKeyJsonConverter()
        }
    };

    private readonly HttpClient _client;
    private readonly bool _isAdmin;

    /// <summary>
    /// Initializes a new instance of <see cref="SIStatisticsServiceClient" /> class.
    /// </summary>
    /// <param name="client">HTTP client to use.</param>
    public SIStatisticsServiceClient(HttpClient client, IOptions<SIStatisticsClientOptions> options)
    {
        _client = client;
        _isAdmin = !string.IsNullOrEmpty(options.Value.ClientSecret);
    }

    public Task<GamesResponse?> GetLatestGamesInfoAsync(StatisticFilter filter, CancellationToken cancellationToken = default) =>
        GetJsonAsync<GamesResponse>("games/results", BuildFilter(filter), cancellationToken);

    public Task<GamesStatistic?> GetLatestGamesStatisticAsync(StatisticFilter filter, CancellationToken cancellationToken = default) =>
        GetJsonAsync<GamesStatistic>("games/stats", BuildFilter(filter), cancellationToken);

    public Task<PackagesStatistic?> GetLatestTopPackagesAsync(TopPackagesRequest request, CancellationToken cancellationToken = default) =>
        GetJsonAsync<PackagesStatistic>("games/packages", BuildRequest(request), cancellationToken);

    public Task<QuestionInfoResponse?> GetQuestionInfoAsync(string themeName, string questionText, CancellationToken cancellationToken = default) =>
        GetJsonAsync<QuestionInfoResponse>(
            "admin/questions",
            new[]
            {
                new KeyValuePair<string, string?>("themeName", themeName),
                new KeyValuePair<string, string?>("questionText", questionText)
            },
            cancellationToken);

    public async Task SendGameReportAsync(GameReport gameReport, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(_isAdmin ? "admin/reports" : "games/reports", gameReport, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await GetErrorAsync(response, cancellationToken);
        }
    }

    public async Task<PackageImportResult?> SendPackageContentAsync(Stream packageContentStream, CancellationToken cancellationToken = default)
    {
        var formData = new MultipartFormDataContent();
        var packageContent = new StreamContent(packageContentStream, BufferSize);
        formData.Add(packageContent, "file", "filename");

        var response = await _client.PostAsync("admin/packages", formData, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await GetErrorAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<PackageImportResult>(SerializerOptions, cancellationToken);
    }

    public Task<PackageStats?> GetPackageStats(PackageStatsRequest request, CancellationToken cancellationToken = default) =>
        GetJsonAsync<PackageStats>(
            "games/packages/stats",
            BuildRequest(request),
            cancellationToken);

    public Task<PackageInfoResponse?> GetPackageInfo(PackageInfoRequest request, CancellationToken cancellationToken = default) =>
        GetJsonAsync<PackageInfoResponse>(
            "games/packages/info",
            BuildRequest(request),
            cancellationToken);

    private Task<T?> GetJsonAsync<T>(string uri, IEnumerable<KeyValuePair<string, string?>> parameters, CancellationToken cancellationToken)
    {
        var queryString = string.Join("&", parameters
            .Where(p => p.Value != null)
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}"));

        return _client.GetFromJsonAsync<T>(uri + (queryString.Length > 0 ? '?' + queryString : ""), cancellationToken);
    }

    private static List<KeyValuePair<string, string?>> BuildFilter(StatisticFilter filter)
    {
        var result = new List<KeyValuePair<string, string?>>()
        {
            new("platform", filter.Platform.ToString()),
            new("from", filter.From.ToString("yyyy-MM-ddTHH:mm:sszzz")),
            new("to", filter.To.ToString("yyyy-MM-ddTHH:mm:sszzz"))
        };

        if (filter.Count.HasValue)
        {
            result.Add(new("count", filter.Count.Value.ToString()));
        }

        if (filter.LanguageCode != null)
        {
            result.Add(new("languageCode", filter.LanguageCode));
        }

        return result;
    }

    private static List<KeyValuePair<string, string?>> BuildRequest(TopPackagesRequest request)
    {
        var filter = BuildFilter(request.StatisticFilter);

        if (request.Source != null)
        {
            filter.Add(new("source", request.Source.ToString()));
        }

        if (request.FallbackSource != null)
        {
            filter.Add(new("fallbackSource", request.FallbackSource.ToString()));
        }

        return filter;
    }

    private static List<KeyValuePair<string, string?>> BuildRequest(PackageStatsRequest request)
    {
        var parameters = new List<KeyValuePair<string, string?>>
        {
            new("name", request.Name),
            new("hash", request.Hash)
        };

        foreach (var author in request.Authors)
        {
            parameters.Add(new("authors", author));
        }

        return parameters;
    }

    private static List<KeyValuePair<string, string?>> BuildRequest(PackageInfoRequest request)
    {
        var parameters = BuildRequest(new PackageStatsRequest(request.Name, request.Hash, request.Authors));

        if (request.Source != null)
        {
            parameters.Add(new("source", request.Source.ToString()));
        }

        parameters.Add(new("includeStats", request.IncludeStats.ToString()));

        return parameters;
    }

    private static async Task<SIStatisticsClientException> GetErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var serverError = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var error = JsonSerializer.Deserialize<SIStatisticServiceError>(serverError, SerializerOptions);

            if (error != null && error.ErrorCode != WellKnownSIStatisticServiceErrorCode.Unknown)
            {
                return new SIStatisticsClientException { ErrorCode = error.ErrorCode, StatusCode = response.StatusCode };
            }
        }
        catch // Invalid JSON or wrong type
        {

        }

        return new SIStatisticsClientException(serverError) { StatusCode = response.StatusCode };
    }
}
