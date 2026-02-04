using System.Text.Json;
using MealPlanner.Api.Localization;
using MealPlanner.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly ErrorLocalizer _errorLocalizer;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        ErrorLocalizer errorLocalizer)
    {
        _logger = logger;
        _environment = environment;
        _errorLocalizer = errorLocalizer;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items["CorrelationId"]?.ToString()
            ?? httpContext.TraceIdentifier;

        return exception switch
        {
            ValidationException validationException => await HandleValidationExceptionAsync(
                httpContext, validationException, correlationId, cancellationToken),
            _ => await HandleUnexpectedExceptionAsync(
                httpContext, exception, correlationId, cancellationToken)
        };
    }

    private async Task<bool> HandleValidationExceptionAsync(
        HttpContext httpContext,
        ValidationException exception,
        string correlationId,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Validation failed. CorrelationId: {CorrelationId}, RequestPath: {RequestPath}, Errors: {@ValidationErrors}",
            correlationId,
            httpContext.Request.Path,
            exception.Errors);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = _errorLocalizer.LocalizeByKey("ProblemDetails.ValidationTitle"),
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Instance = httpContext.Request.Path,
            Detail = _errorLocalizer.LocalizeByKey("ProblemDetails.ValidationDetail"),
            Extensions =
            {
                ["correlationId"] = correlationId,
                ["traceId"] = httpContext.TraceIdentifier,
                ["errors"] = exception.Errors
            }
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            cancellationToken);

        return true;
    }

    private async Task<bool> HandleUnexpectedExceptionAsync(
        HttpContext httpContext,
        Exception exception,
        string correlationId,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, RequestPath: {RequestPath}, RequestMethod: {RequestMethod}",
            correlationId,
            httpContext.Request.Path,
            httpContext.Request.Method);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = _errorLocalizer.LocalizeByKey("ProblemDetails.UnexpectedTitle"),
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["correlationId"] = correlationId,
                ["traceId"] = httpContext.TraceIdentifier
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["exception"] = new
            {
                message = exception.Message,
                type = exception.GetType().Name,
                stackTrace = exception.StackTrace
            };
        }
        else
        {
            problemDetails.Detail = _errorLocalizer.LocalizeByKey("ProblemDetails.UnexpectedDetail");
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            cancellationToken);

        return true;
    }
}
