using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;

namespace MealPlanner.Api.Middleware;

public static class RateLimitingMiddleware
{
    public static ValueTask OnRejected(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

        var correlationId = httpContext.Items.TryGetValue("CorrelationId", out var id)
            ? id?.ToString()
            : null;

        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = $"{httpContext.Request.Method} {httpContext.Request.Path}";

        logger.LogWarning(
            "Rate limit exceeded for {ClientIp} on {Endpoint} (CorrelationId: {CorrelationId})",
            clientIp,
            endpoint,
            correlationId);

        var retryAfterSeconds = 60;
        httpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

        httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            title = "Too Many Requests",
            status = StatusCodes.Status429TooManyRequests,
            detail = "Rate limit exceeded. Please try again later.",
            instance = httpContext.Request.Path.Value,
            traceId = correlationId ?? httpContext.TraceIdentifier,
            retryAfterSeconds
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        return new ValueTask(httpContext.Response.WriteAsJsonAsync(problemDetails, options, cancellationToken));
    }
}
