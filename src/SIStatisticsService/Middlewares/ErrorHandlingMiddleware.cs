using SIStatisticsService.Contract.Models;
using SIStatisticsService.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SIStatisticsService.Middlewares;

/// <summary>
/// Handles exceptions and creates corresponsing service responses.
/// </summary>
internal sealed class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
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
