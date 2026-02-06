using System.Text.RegularExpressions;

namespace MealPlanner.Api.Tests.Documentation;

/// <summary>
/// Extracts API endpoints from Program.cs source code by parsing MapXxx method calls.
/// </summary>
public sealed class EndpointCodeExtractor
{
    /// <summary>
    /// Pattern to match app.MapXxx calls in Program.cs.
    /// Captures HTTP method from MapGet/MapPost/etc. and the route path.
    /// Examples:
    /// - app.MapGet("/health/live", ...)
    /// - app.MapPost("/api/v1/auth/login", async ...)
    /// </summary>
    private static readonly Regex EndpointPattern = new(
        @"app\.Map(Get|Post|Put|Patch|Delete)\s*\(\s*""([^""]+)""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Pattern to match health check mappings.
    /// Example: app.MapHealthChecks("/health/live", ...)
    /// </summary>
    private static readonly Regex HealthCheckPattern = new(
        @"app\.MapHealthChecks\s*\(\s*""([^""]+)""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Extracts all endpoint definitions from Program.cs source code.
    /// </summary>
    /// <param name="sourceCode">The content of Program.cs.</param>
    /// <returns>A list of endpoints defined in code.</returns>
    public IReadOnlyList<EndpointInfo> Extract(string sourceCode)
    {
        ArgumentNullException.ThrowIfNull(sourceCode);

        var endpoints = new List<EndpointInfo>();

        // Extract MapXxx endpoints
        var mapMatches = EndpointPattern.Matches(sourceCode);
        foreach (Match match in mapMatches)
        {
            var httpMethod = match.Groups[1].Value.ToUpperInvariant();
            var routePath = match.Groups[2].Value;
            endpoints.Add(new EndpointInfo(httpMethod, routePath));
        }

        // Extract health check endpoints (GET method)
        var healthCheckMatches = HealthCheckPattern.Matches(sourceCode);
        foreach (Match match in healthCheckMatches)
        {
            var routePath = match.Groups[1].Value;
            endpoints.Add(new EndpointInfo("GET", routePath));
        }

        return endpoints;
    }

    /// <summary>
    /// Extracts endpoints from Program.cs at the specified file path.
    /// </summary>
    /// <param name="filePath">The path to Program.cs.</param>
    /// <returns>A list of endpoints defined in code.</returns>
    public IReadOnlyList<EndpointInfo> ExtractFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Source file not found: {filePath}", filePath);
        }

        var content = File.ReadAllText(filePath);
        return Extract(content);
    }
}
