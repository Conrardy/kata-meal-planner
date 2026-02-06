namespace MealPlanner.Api.Tests.Documentation;

/// <summary>
/// Report of synchronization between documented and implemented endpoints.
/// </summary>
public sealed class EndpointSynchronizationReport
{
    /// <summary>
    /// Endpoints that are documented but not found in code.
    /// </summary>
    public IReadOnlyList<EndpointInfo> DocumentedButMissingInCode { get; init; } = [];

    /// <summary>
    /// Endpoints that are in code but not documented.
    /// </summary>
    public IReadOnlyList<EndpointInfo> InCodeButUndocumented { get; init; } = [];

    /// <summary>
    /// Endpoints that are synchronized (exist in both documentation and code).
    /// </summary>
    public IReadOnlyList<EndpointInfo> Synchronized { get; init; } = [];

    /// <summary>
    /// Total number of documented endpoints.
    /// </summary>
    public int TotalDocumented { get; init; }

    /// <summary>
    /// Total number of endpoints in code.
    /// </summary>
    public int TotalInCode { get; init; }

    /// <summary>
    /// Returns true if documentation and code are fully synchronized.
    /// </summary>
    public bool IsFullySynchronized =>
        DocumentedButMissingInCode.Count == 0 && InCodeButUndocumented.Count == 0;

    /// <summary>
    /// Generates a human-readable report.
    /// </summary>
    public string GenerateReport()
    {
        var lines = new List<string>
        {
            "# Endpoint Synchronization Report",
            "",
            $"**Generated at:** {DateTime.UtcNow:O}",
            "",
            "## Summary",
            "",
            $"| Metric | Count |",
            $"|--------|-------|",
            $"| Total Documented | {TotalDocumented} |",
            $"| Total In Code | {TotalInCode} |",
            $"| Synchronized | {Synchronized.Count} |",
            $"| Documented but Missing | {DocumentedButMissingInCode.Count} |",
            $"| Undocumented | {InCodeButUndocumented.Count} |",
            ""
        };

        if (DocumentedButMissingInCode.Count > 0)
        {
            lines.Add("## Documented but Missing in Code");
            lines.Add("");
            lines.Add("These endpoints are documented but **no longer exist in the code**:");
            lines.Add("");
            lines.Add("| Method | Route |");
            lines.Add("|--------|-------|");
            foreach (var endpoint in DocumentedButMissingInCode)
            {
                lines.Add($"| `{endpoint.HttpMethod}` | `{endpoint.RoutePath}` |");
            }
            lines.Add("");
        }

        if (InCodeButUndocumented.Count > 0)
        {
            lines.Add("## Undocumented Endpoints");
            lines.Add("");
            lines.Add("These endpoints exist in code but are **not documented**:");
            lines.Add("");
            lines.Add("| Method | Route |");
            lines.Add("|--------|-------|");
            foreach (var endpoint in InCodeButUndocumented)
            {
                lines.Add($"| `{endpoint.HttpMethod}` | `{endpoint.RoutePath}` |");
            }
            lines.Add("");
        }

        if (IsFullySynchronized)
        {
            lines.Add("## Status: SYNCHRONIZED");
            lines.Add("");
            lines.Add("All endpoints are documented and all documented endpoints exist in code.");
        }
        else
        {
            lines.Add("## Status: OUT OF SYNC");
            lines.Add("");
            lines.Add("Documentation needs to be updated to match the codebase.");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
