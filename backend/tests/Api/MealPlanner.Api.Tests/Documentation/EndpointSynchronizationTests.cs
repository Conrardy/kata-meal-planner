using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace MealPlanner.Api.Tests.Documentation;

/// <summary>
/// Tests that verify documentation is synchronized with the codebase.
/// </summary>
public sealed class EndpointSynchronizationTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _documentationPath;
    private readonly string _programCsPath;

    public EndpointSynchronizationTests(ITestOutputHelper output)
    {
        _output = output;

        // Navigate from test project to docs/api/endpoints.md
        var testProjectDir = AppContext.BaseDirectory;
        var solutionRoot = FindSolutionRoot(testProjectDir);

        _documentationPath = Path.Combine(solutionRoot, "docs", "api", "endpoints.md");
        _programCsPath = Path.Combine(solutionRoot, "backend", "src", "Api", "MealPlanner.Api", "Program.cs");
    }

    [Fact]
    public void AllDocumentedEndpoints_ShouldExistInCode()
    {
        // Arrange
        var synchronizer = new EndpointSynchronizer();

        // Act
        var report = synchronizer.CompareFromFiles(_documentationPath, _programCsPath);

        // Assert - Log report for CI visibility
        _output.WriteLine(report.GenerateReport());

        if (report.DocumentedButMissingInCode.Count > 0)
        {
            var missingEndpoints = string.Join(Environment.NewLine,
                report.DocumentedButMissingInCode.Select(e => $"  - {e}"));

            _output.WriteLine($"""

                ERROR: The following documented endpoints no longer exist in code:
                {missingEndpoints}

                Action required: Remove outdated documentation from docs/api/endpoints.md
                """);
        }

        report.DocumentedButMissingInCode.Should().BeEmpty(
            "all documented endpoints should exist in the codebase. " +
            "Remove or update documentation for endpoints that no longer exist.");
    }

    [Fact]
    public void AllCodeEndpoints_ShouldBeDocumented()
    {
        // Arrange
        var synchronizer = new EndpointSynchronizer();

        // Act
        var report = synchronizer.CompareFromFiles(_documentationPath, _programCsPath);

        // Assert - Log report for CI visibility
        _output.WriteLine(report.GenerateReport());

        if (report.InCodeButUndocumented.Count > 0)
        {
            var undocumentedEndpoints = string.Join(Environment.NewLine,
                report.InCodeButUndocumented.Select(e => $"  - {e}"));

            _output.WriteLine($"""

                WARNING: The following endpoints are not documented:
                {undocumentedEndpoints}

                Action: Run the skeleton generator to create documentation templates.
                """);
        }

        // This test ensures all code endpoints are documented
        report.InCodeButUndocumented.Should().BeEmpty(
            "all endpoints in code should be documented in docs/api/endpoints.md. " +
            "Use the DocumentationSkeletonGenerator to create templates for new endpoints.");
    }

    [Fact]
    public void Documentation_ShouldBeSynchronizedWithCode()
    {
        // Arrange
        var synchronizer = new EndpointSynchronizer();

        // Act
        var report = synchronizer.CompareFromFiles(_documentationPath, _programCsPath);

        // Assert
        _output.WriteLine(report.GenerateReport());

        report.IsFullySynchronized.Should().BeTrue(
            "documentation should be fully synchronized with the codebase. " +
            $"Found {report.DocumentedButMissingInCode.Count} documented but missing, " +
            $"{report.InCodeButUndocumented.Count} undocumented.");
    }

    [Fact]
    public void GenerateSkeletonForUndocumentedEndpoints()
    {
        // Arrange
        var synchronizer = new EndpointSynchronizer();
        var generator = new DocumentationSkeletonGenerator();

        // Act
        var report = synchronizer.CompareFromFiles(_documentationPath, _programCsPath);
        var skeleton = generator.Generate(report.InCodeButUndocumented);

        // Assert - Always output for reference
        _output.WriteLine("=== Documentation Skeleton for Undocumented Endpoints ===");
        _output.WriteLine(skeleton);

        // This test always passes - it's a helper to generate documentation
        report.Should().NotBeNull();
    }

    [Fact]
    public void DocumentationParser_ShouldParse23Endpoints()
    {
        // Arrange
        var parser = new EndpointDocumentationParser();

        // Act
        var endpoints = parser.ParseFromFile(_documentationPath);

        // Assert
        _output.WriteLine($"Parsed {endpoints.Count} documented endpoints:");
        foreach (var endpoint in endpoints)
        {
            _output.WriteLine($"  - {endpoint}");
        }

        endpoints.Count.Should().Be(23,
            "the documentation states there are 23 endpoints (check 'Total : 23 endpoints' in endpoints.md)");
    }

    [Fact]
    public void CodeExtractor_ShouldExtractAllEndpointsFromProgramCs()
    {
        // Arrange
        var extractor = new EndpointCodeExtractor();

        // Act
        var endpoints = extractor.ExtractFromFile(_programCsPath);

        // Assert
        _output.WriteLine($"Extracted {endpoints.Count} endpoints from Program.cs:");
        foreach (var endpoint in endpoints)
        {
            _output.WriteLine($"  - {endpoint}");
        }

        endpoints.Count.Should().BeGreaterThan(0, "Program.cs should contain endpoint definitions");
    }

    private static string FindSolutionRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory != null)
        {
            // Look for CLAUDE.md or .git as indicators of solution root
            if (File.Exists(Path.Combine(directory.FullName, "CLAUDE.md")) ||
                Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find solution root from {startPath}. " +
            "Ensure CLAUDE.md or .git exists at the repository root.");
    }
}
