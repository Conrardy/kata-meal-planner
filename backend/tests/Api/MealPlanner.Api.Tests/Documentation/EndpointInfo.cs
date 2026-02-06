namespace MealPlanner.Api.Tests.Documentation;

/// <summary>
/// Represents an API endpoint with its HTTP method and route path.
/// </summary>
public sealed record EndpointInfo(string HttpMethod, string RoutePath)
{
    /// <summary>
    /// Normalizes the route path for comparison by:
    /// - Trimming trailing slashes
    /// - Converting to lowercase
    /// - Replacing path parameters with placeholders
    /// </summary>
    public string NormalizedPath => NormalizePath(RoutePath);

    /// <summary>
    /// Returns a unique key for comparison (METHOD + normalized path).
    /// </summary>
    public string Key => $"{HttpMethod.ToUpperInvariant()} {NormalizedPath}";

    private static string NormalizePath(string path)
    {
        var normalized = path.TrimEnd('/').ToLowerInvariant();

        // Replace all path parameters (simple or typed) with {param} for consistent comparison
        // Examples: {recipeId}, {date}, {mealId:guid}, {startDate:datetime} all become {param}
        normalized = System.Text.RegularExpressions.Regex.Replace(
            normalized,
            @"\{[^}]+\}",
            "{param}");

        return normalized;
    }

    public override string ToString() => $"{HttpMethod} {RoutePath}";
}
