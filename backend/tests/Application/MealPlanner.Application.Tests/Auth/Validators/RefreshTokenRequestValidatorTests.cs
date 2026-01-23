using FluentAssertions;
using MealPlanner.Application.Auth;
using MealPlanner.Application.Auth.Validators;
using Xunit;

namespace MealPlanner.Application.Tests.Auth.Validators;

public sealed class RefreshTokenRequestValidatorTests
{
    private readonly RefreshTokenRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidToken_ShouldPass()
    {
        // Arrange
        var request = new RefreshTokenRequest("valid-refresh-token");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyToken_ShouldFail(string? token)
    {
        // Arrange
        var request = new RefreshTokenRequest(token!);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RefreshToken");
    }
}
