using FluentAssertions;
using Xunit;

namespace MealPlanner.Api.Tests.Documentation;

public sealed class EndpointInfoTests
{
    [Theory]
    [InlineData("/api/v1/recipes", "/api/v1/recipes")]
    [InlineData("/api/v1/recipes/", "/api/v1/recipes")]
    [InlineData("/API/V1/RECIPES", "/api/v1/recipes")]
    public void NormalizedPath_RemovesTrailingSlashAndLowercases(string input, string expected)
    {
        // Arrange
        var endpoint = new EndpointInfo("GET", input);

        // Act & Assert
        endpoint.NormalizedPath.Should().Be(expected);
    }

    [Theory]
    [InlineData("/api/v1/recipes/{recipeId}", "/api/v1/recipes/{param}")]
    [InlineData("/api/v1/meals/{mealId}/suggestions", "/api/v1/meals/{param}/suggestions")]
    [InlineData("/api/v1/shopping-list/{startDate}/items/{itemId}", "/api/v1/shopping-list/{param}/items/{param}")]
    public void NormalizedPath_ReplacesPathParametersWithPlaceholder(string input, string expected)
    {
        // Arrange
        var endpoint = new EndpointInfo("GET", input);

        // Act & Assert
        endpoint.NormalizedPath.Should().Be(expected);
    }

    [Theory]
    [InlineData("/api/v1/recipes/{recipeId:guid}", "/api/v1/recipes/{param}")]
    public void NormalizedPath_HandlesTypedGuidParameters(string input, string expected)
    {
        // Arrange - typed constraints like :guid are normalized to {param} for consistent comparison
        var endpoint = new EndpointInfo("GET", input);

        // Act & Assert
        endpoint.NormalizedPath.Should().Be(expected);
    }

    [Theory]
    [InlineData("/api/v1/daily-digest/{date:datetime}", "/api/v1/daily-digest/{param}")]
    public void NormalizedPath_HandlesTypedDateTimeParameters(string input, string expected)
    {
        // Arrange
        var endpoint = new EndpointInfo("GET", input);

        // Act & Assert
        endpoint.NormalizedPath.Should().Be(expected);
    }

    [Fact]
    public void Key_CombinesMethodAndNormalizedPath()
    {
        // Arrange
        var endpoint = new EndpointInfo("post", "/api/v1/recipes");

        // Act
        var key = endpoint.Key;

        // Assert
        key.Should().Be("POST /api/v1/recipes");
    }

    [Fact]
    public void SameEndpoints_HaveSameKey()
    {
        // Arrange
        var endpoint1 = new EndpointInfo("GET", "/api/v1/recipes/{recipeId}");
        var endpoint2 = new EndpointInfo("get", "/api/v1/recipes/{id}");

        // Act & Assert
        endpoint1.Key.Should().Be(endpoint2.Key);
    }

    [Fact]
    public void DifferentMethods_HaveDifferentKeys()
    {
        // Arrange
        var getEndpoint = new EndpointInfo("GET", "/api/v1/recipes");
        var postEndpoint = new EndpointInfo("POST", "/api/v1/recipes");

        // Act & Assert
        getEndpoint.Key.Should().NotBe(postEndpoint.Key);
    }

    [Fact]
    public void DifferentPaths_HaveDifferentKeys()
    {
        // Arrange
        var recipes = new EndpointInfo("GET", "/api/v1/recipes");
        var preferences = new EndpointInfo("GET", "/api/v1/preferences");

        // Act & Assert
        recipes.Key.Should().NotBe(preferences.Key);
    }

    [Fact]
    public void ToString_ReturnsReadableFormat()
    {
        // Arrange
        var endpoint = new EndpointInfo("GET", "/api/v1/recipes/{recipeId}");

        // Act
        var result = endpoint.ToString();

        // Assert
        result.Should().Be("GET /api/v1/recipes/{recipeId}");
    }
}
