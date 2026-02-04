using ErrorOr;
using MealPlanner.Api.Localization;
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

        var errorLocalizer = httpContext.RequestServices.GetService<ErrorLocalizer>();

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            return CreateValidationProblem(errors, httpContext, logger, correlationId, errorLocalizer);
        }

        var firstError = errors.First();
        return CreateProblemFromError(firstError, httpContext, logger, correlationId, errors, errorLocalizer);
    }

    private static IResult CreateValidationProblem(
        IList<Error> errors,
        HttpContext httpContext,
        ILogger? logger,
        string correlationId,
        ErrorLocalizer? errorLocalizer)
    {
        var validationErrors = errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => LocalizeErrorDescription(e, errorLocalizer)).ToArray());

        logger?.LogWarning(
            "Validation errors occurred. CorrelationId: {CorrelationId}, Errors: {@ValidationErrors}",
            correlationId,
            validationErrors);

        var problemDetails = ApiProblemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            validationErrors,
            errorLocalizer);

        return Results.Problem(problemDetails);
    }

    private static IResult CreateProblemFromError(
        Error error,
        HttpContext httpContext,
        ILogger? logger,
        string correlationId,
        IList<Error> allErrors,
        ErrorLocalizer? errorLocalizer)
    {
        var (statusCode, titleKey) = error.Type switch
        {
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "ProblemDetails.Conflict"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "ProblemDetails.NotFound"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "ProblemDetails.Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "ProblemDetails.Forbidden"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "ProblemDetails.BadRequest"),
            _ => (StatusCodes.Status500InternalServerError, "ProblemDetails.UnexpectedTitle")
        };

        var title = errorLocalizer?.LocalizeByKey(titleKey) ?? titleKey;
        var localizedDescription = LocalizeErrorDescription(error, errorLocalizer);

        logger?.LogWarning(
            "Business error occurred. CorrelationId: {CorrelationId}, ErrorType: {ErrorType}, ErrorCode: {ErrorCode}, ErrorDescription: {ErrorDescription}",
            correlationId,
            error.Type,
            error.Code,
            error.Description);

        var errorDetails = allErrors.Count > 1
            ? allErrors.ToDictionary(e => e.Code, e => (object?)LocalizeErrorDescription(e, errorLocalizer))
            : null;

        var problemDetails = ApiProblemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode,
            title,
            localizedDescription,
            errors: errorDetails);

        return Results.Problem(problemDetails);
    }

    private static string LocalizeErrorDescription(Error error, ErrorLocalizer? errorLocalizer)
    {
        if (errorLocalizer is null)
            return error.Description;

        return errorLocalizer.Localize(error.Code, error.Description);
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
