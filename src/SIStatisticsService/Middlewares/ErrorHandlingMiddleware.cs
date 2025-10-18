using SIStatisticsService.Contract.Models;
using SIStatisticsService.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SIStatisticsService.Middlewares;

/// <summary>
/// Handles exceptions and creates corresponsing service responses.
/// </summary>
internal sealed class ErrorHandlingMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ServiceException exc)
        {
            context.Response.StatusCode = (int)exc.StatusCode;

            await context.Response.WriteAsJsonAsync(
                new SIStatisticServiceError { ErrorCode = exc.ErrorCode },
                SerializerOptions);
        }
    }
}
