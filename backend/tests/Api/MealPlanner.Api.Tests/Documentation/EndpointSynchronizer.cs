namespace MealPlanner.Api.Tests.Documentation;

/// <summary>
/// Compares documented endpoints with code endpoints and generates a synchronization report.
/// </summary>
public sealed class EndpointSynchronizer
{
    private readonly EndpointDocumentationParser _documentationParser;
    private readonly EndpointCodeExtractor _codeExtractor;

    public EndpointSynchronizer()
    {
        _documentationParser = new EndpointDocumentationParser();
        _codeExtractor = new EndpointCodeExtractor();
    }

    /// <summary>
    /// Compares endpoints from documentation and code sources.
    /// </summary>
    /// <param name="documentedEndpoints">Endpoints from documentation.</param>
    /// <param name="codeEndpoints">Endpoints from code.</param>
    /// <returns>A synchronization report.</returns>
    public EndpointSynchronizationReport Compare(
        IReadOnlyList<EndpointInfo> documentedEndpoints,
        IReadOnlyList<EndpointInfo> codeEndpoints)
    {
        var documentedKeys = documentedEndpoints
            .Select(e => e.Key)
            .ToHashSet();

        var codeKeys = codeEndpoints
            .Select(e => e.Key)
            .ToHashSet();

        var documentedButMissing = documentedEndpoints
            .Where(e => !codeKeys.Contains(e.Key))
            .ToList();

        var undocumented = codeEndpoints
            .Where(e => !documentedKeys.Contains(e.Key))
            .ToList();

        var synchronized = documentedEndpoints
            .Where(e => codeKeys.Contains(e.Key))
            .ToList();

        return new EndpointSynchronizationReport
        {
            DocumentedButMissingInCode = documentedButMissing,
            InCodeButUndocumented = undocumented,
            Synchronized = synchronized,
            TotalDocumented = documentedEndpoints.Count,
            TotalInCode = codeEndpoints.Count
        };
    }

    /// <summary>
    /// Compares endpoints from documentation and code files.
    /// </summary>
    /// <param name="documentationPath">Path to endpoints.md.</param>
    /// <param name="programCsPath">Path to Program.cs.</param>
    /// <returns>A synchronization report.</returns>
    public EndpointSynchronizationReport CompareFromFiles(string documentationPath, string programCsPath)
    {
        var documentedEndpoints = _documentationParser.ParseFromFile(documentationPath);
        var codeEndpoints = _codeExtractor.ExtractFromFile(programCsPath);
        return Compare(documentedEndpoints, codeEndpoints);
    }
}
