using ErrorOr;
using MealPlanner.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Extensions;

public static class ErrorOrExtensions
{
    public static IResult ToProblemResult(
        this IList<Error> errors,
        HttpContext httpContext,
        ILogger? logger = null)
    {
        if (errors.Count == 0)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An unexpected error occurred");
        }

        var correlationId = httpContext.Items["CorrelationId"]?.ToString()
            ?? httpContext.TraceIdentifier;

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            return CreateValidationProblem(errors, httpContext, logger, correlationId);
        }

        var firstError = errors.First();
        return CreateProblemFromError(firstError, httpContext, logger, correlationId, errors);
    }

    private static IResult CreateValidationProblem(
        IList<Error> errors,
        HttpContext httpContext,
        ILogger? logger,
        string correlationId)
    {
        var validationErrors = errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray());

        logger?.LogWarning(
            "Validation errors occurred. CorrelationId: {CorrelationId}, Errors: {@ValidationErrors}",
            correlationId,
            validationErrors);

        var problemDetails = ApiProblemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            validationErrors);

        return Results.Problem(problemDetails);
    }

    private static IResult CreateProblemFromError(
        Error error,
        HttpContext httpContext,
        ILogger? logger,
        string correlationId,
        IList<Error> allErrors)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        logger?.LogWarning(
            "Business error occurred. CorrelationId: {CorrelationId}, ErrorType: {ErrorType}, ErrorCode: {ErrorCode}, ErrorDescription: {ErrorDescription}",
            correlationId,
            error.Type,
            error.Code,
            error.Description);

        var errorDetails = allErrors.Count > 1
            ? allErrors.ToDictionary(e => e.Code, e => (object?)e.Description)
            : null;

        var problemDetails = ApiProblemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode,
            title,
            error.Description,
            errors: errorDetails);

        return Results.Problem(problemDetails);
    }

    public static IResult MatchResult<T>(
        this ErrorOr<T> errorOr,
        HttpContext httpContext,
        Func<T, IResult> onSuccess,
        ILogger? logger = null)
    {
        return errorOr.Match(
            onSuccess,
            errors => errors.ToProblemResult(httpContext, logger));
    }

    public static async Task<IResult> MatchResultAsync<T>(
        this Task<ErrorOr<T>> errorOrTask,
        HttpContext httpContext,
        Func<T, IResult> onSuccess,
        ILogger? logger = null)
    {
        var errorOr = await errorOrTask;
        return errorOr.MatchResult(httpContext, onSuccess, logger);
    }
}
