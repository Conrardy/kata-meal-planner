using FluentAssertions;
using Xunit;

namespace MealPlanner.Api.Tests.Documentation;

public sealed class EndpointDocumentationParserTests
{
    private readonly EndpointDocumentationParser _parser;

    public EndpointDocumentationParserTests()
    {
        _parser = new EndpointDocumentationParser();
    }

    [Fact]
    public void Parse_WithGetEndpoint_ExtractsCorrectly()
    {
        // Arrange
        var markdown = """
            ### GET `/health/live` - Liveness Check

            Some description here.
            """;

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("GET");
        endpoints[0].RoutePath.Should().Be("/health/live");
    }

    [Fact]
    public void Parse_WithPostEndpoint_ExtractsCorrectly()
    {
        // Arrange
        var markdown = """
            ### POST `/api/v1/auth/login` - Connexion

            Login endpoint.
            """;

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("POST");
        endpoints[0].RoutePath.Should().Be("/api/v1/auth/login");
    }

    [Fact]
    public void Parse_WithPathParameters_ExtractsCorrectly()
    {
        // Arrange
        var markdown = """
            ### GET `/api/v1/recipes/{recipeId}` - Details

            Recipe details endpoint.
            """;

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("GET");
        endpoints[0].RoutePath.Should().Be("/api/v1/recipes/{recipeId}");
    }

    [Fact]
    public void Parse_WithMultipleEndpoints_ExtractsAll()
    {
        // Arrange
        var markdown = """
            ## Health

            ### GET `/health/live` - Liveness

            Liveness check.

            ### GET `/health/ready` - Readiness

            Readiness check.

            ## Auth

            ### POST `/api/v1/auth/login` - Login

            Login endpoint.
            """;

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().HaveCount(3);
        endpoints.Should().Contain(e => e.HttpMethod == "GET" && e.RoutePath == "/health/live");
        endpoints.Should().Contain(e => e.HttpMethod == "GET" && e.RoutePath == "/health/ready");
        endpoints.Should().Contain(e => e.HttpMethod == "POST" && e.RoutePath == "/api/v1/auth/login");
    }

    [Fact]
    public void Parse_WithPatchAndDelete_ExtractsCorrectly()
    {
        // Arrange
        var markdown = """
            ### PATCH `/api/v1/items/{id}` - Update

            Partial update.

            ### DELETE `/api/v1/items/{id}` - Remove

            Delete item.
            """;

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().HaveCount(2);
        endpoints.Should().Contain(e => e.HttpMethod == "PATCH" && e.RoutePath == "/api/v1/items/{id}");
        endpoints.Should().Contain(e => e.HttpMethod == "DELETE" && e.RoutePath == "/api/v1/items/{id}");
    }

    [Fact]
    public void Parse_WithPutEndpoint_ExtractsCorrectly()
    {
        // Arrange
        var markdown = """
            ### PUT `/api/v1/preferences` - Update preferences

            Full update of preferences.
            """;

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().ContainSingle();
        endpoints[0].HttpMethod.Should().Be("PUT");
        endpoints[0].RoutePath.Should().Be("/api/v1/preferences");
    }

    [Fact]
    public void Parse_WithEmptyMarkdown_ReturnsEmptyList()
    {
        // Arrange
        var markdown = "";

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithNoEndpoints_ReturnsEmptyList()
    {
        // Arrange
        var markdown = """
            # API Documentation

            This is some general documentation without any endpoint definitions.

            ## Overview

            Some overview text.
            """;

        // Act
        var endpoints = _parser.Parse(markdown);

        // Assert
        endpoints.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithNullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _parser.Parse(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
