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
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
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
            new Dictionary<string, object>
            {
                ["themeName"] = themeName,
                ["questionText"] = questionText
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

    public async Task SendPackageContentAsync(Stream packageContentStream, CancellationToken cancellationToken = default)
    {
        var formData = new MultipartFormDataContent();
        var packageContent = new StreamContent(packageContentStream, BufferSize);
        formData.Add(packageContent, "file", "filename");

        var response = await _client.PostAsync("admin/packages", formData, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await GetErrorAsync(response, cancellationToken);
        }
    }

    private Task<T?> GetJsonAsync<T>(string uri, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value.ToString() ?? "")}"));

        return _client.GetFromJsonAsync<T>(uri + (queryString.Length > 0 ? '?' + queryString : ""), cancellationToken);
    }

    private static Dictionary<string, object> BuildFilter(StatisticFilter filter)
    {
        var result = new Dictionary<string, object>()
        {
            ["platform"] = filter.Platform,
            ["from"] = filter.From.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            ["to"] = filter.To.ToString("yyyy-MM-ddTHH:mm:sszzz")
        };

        if (filter.Count.HasValue)
        {
            result["count"] = filter.Count.Value;
        }

        if (filter.LanguageCode != null)
        {
            result["languageCode"] = filter.LanguageCode;
        }

        return result;
    }

    private static Dictionary<string, object> BuildRequest(TopPackagesRequest request)
    {
        var filter = BuildFilter(request.StatisticFilter);
        
        if (request.Source != null)
        {
            filter["source"] = request.Source.ToString();
        }

        return filter;
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
