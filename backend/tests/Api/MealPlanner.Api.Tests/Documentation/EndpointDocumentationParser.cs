using System.Text.RegularExpressions;

namespace MealPlanner.Api.Tests.Documentation;

/// <summary>
/// Parses the API documentation markdown file to extract documented endpoints.
/// </summary>
public sealed class EndpointDocumentationParser
{
    /// <summary>
    /// Pattern to match endpoint headers like "### GET `/health/live` - Description"
    /// Captures HTTP method and route path.
    /// </summary>
    private static readonly Regex EndpointHeaderPattern = new(
        @"^###\s+(GET|POST|PUT|PATCH|DELETE)\s+`([^`]+)`",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Parses the endpoints.md file and extracts all documented endpoints.
    /// </summary>
    /// <param name="markdownContent">The content of the endpoints.md file.</param>
    /// <returns>A list of documented endpoints.</returns>
    public IReadOnlyList<EndpointInfo> Parse(string markdownContent)
    {
        ArgumentNullException.ThrowIfNull(markdownContent);

        var endpoints = new List<EndpointInfo>();
        var matches = EndpointHeaderPattern.Matches(markdownContent);

        foreach (Match match in matches)
        {
            var httpMethod = match.Groups[1].Value.ToUpperInvariant();
            var routePath = match.Groups[2].Value;
            endpoints.Add(new EndpointInfo(httpMethod, routePath));
        }

        return endpoints;
    }

    /// <summary>
    /// Parses the endpoints.md file from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the endpoints.md file.</param>
    /// <returns>A list of documented endpoints.</returns>
    public IReadOnlyList<EndpointInfo> ParseFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Documentation file not found: {filePath}", filePath);
        }

        var content = File.ReadAllText(filePath);
        return Parse(content);
    }
}
