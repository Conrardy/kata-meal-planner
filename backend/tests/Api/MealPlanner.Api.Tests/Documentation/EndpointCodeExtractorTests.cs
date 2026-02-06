using FluentAssertions;
using Xunit;

namespace MealPlanner.Api.Tests.Documentation;

public sealed class EndpointCodeExtractorTests
{
    private readonly EndpointCodeExtractor _extractor;

    public EndpointCodeExtractorTests()
    {
        _extractor = new EndpointCodeExtractor();
    }

    [Fact]
    public void Extract_WithMapGet_ExtractsCorrectly()
    {
        // Arrange
        var sourceCode = """
            app.MapGet("/api/v1/recipes", async (IMediator mediator) =>
            {
                return Results.Ok();
            });
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("GET");
        endpoints[0].RoutePath.Should().Be("/api/v1/recipes");
    }

    [Fact]
    public void Extract_WithMapPost_ExtractsCorrectly()
    {
        // Arrange
        var sourceCode = """
            app.MapPost("/api/v1/auth/login", async (LoginRequest request) =>
            {
                return Results.Ok();
            });
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("POST");
        endpoints[0].RoutePath.Should().Be("/api/v1/auth/login");
    }

    [Fact]
    public void Extract_WithMapPut_ExtractsCorrectly()
    {
        // Arrange
        var sourceCode = """
            app.MapPut("/api/v1/preferences", async (UpdateRequest request) =>
            {
                return Results.Ok();
            });
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("PUT");
        endpoints[0].RoutePath.Should().Be("/api/v1/preferences");
    }

    [Fact]
    public void Extract_WithMapPatch_ExtractsCorrectly()
    {
        // Arrange
        var sourceCode = """
            app.MapPatch("/api/v1/items/{id}", async (string id, UpdateRequest request) =>
            {
                return Results.NoContent();
            });
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("PATCH");
        endpoints[0].RoutePath.Should().Be("/api/v1/items/{id}");
    }

    [Fact]
    public void Extract_WithMapDelete_ExtractsCorrectly()
    {
        // Arrange
        var sourceCode = """
            app.MapDelete("/api/v1/items/{id}", async (string id) =>
            {
                return Results.NoContent();
            });
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("DELETE");
        endpoints[0].RoutePath.Should().Be("/api/v1/items/{id}");
    }

    [Fact]
    public void Extract_WithHealthCheck_ExtractsAsGet()
    {
        // Arrange
        var sourceCode = """
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("GET");
        endpoints[0].RoutePath.Should().Be("/health/live");
    }

    [Fact]
    public void Extract_WithMultipleEndpoints_ExtractsAll()
    {
        // Arrange
        var sourceCode = """
            app.MapHealthChecks("/health/live", new HealthCheckOptions());

            app.MapGet("/api/v1/recipes", async () => Results.Ok());

            app.MapPost("/api/v1/recipes", async (CreateRecipeRequest request) => Results.Created());

            app.MapDelete("/api/v1/recipes/{id}", async (Guid id) => Results.NoContent());
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().HaveCount(4);
        endpoints.Should().Contain(e => e.HttpMethod == "GET" && e.RoutePath == "/health/live");
        endpoints.Should().Contain(e => e.HttpMethod == "GET" && e.RoutePath == "/api/v1/recipes");
        endpoints.Should().Contain(e => e.HttpMethod == "POST" && e.RoutePath == "/api/v1/recipes");
        endpoints.Should().Contain(e => e.HttpMethod == "DELETE" && e.RoutePath == "/api/v1/recipes/{id}");
    }

    [Fact]
    public void Extract_WithEmptySource_ReturnsEmptyList()
    {
        // Arrange
        var sourceCode = "";

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().BeEmpty();
    }

    [Fact]
    public void Extract_WithNoEndpoints_ReturnsEmptyList()
    {
        // Arrange
        var sourceCode = """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddOpenApi();
            var app = builder.Build();
            app.Run();
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().BeEmpty();
    }

    [Fact]
    public void Extract_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _extractor.Extract(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Extract_WithComplexRoutePattern_ExtractsCorrectly()
    {
        // Arrange
        var sourceCode = """
            app.MapGet("/api/v1/shopping-list/{startDate}/items/{itemId}", async (DateOnly startDate, string itemId) =>
            {
                return Results.Ok();
            });
            """;

        // Act
        var endpoints = _extractor.Extract(sourceCode);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].RoutePath.Should().Be("/api/v1/shopping-list/{startDate}/items/{itemId}");
    }
}
