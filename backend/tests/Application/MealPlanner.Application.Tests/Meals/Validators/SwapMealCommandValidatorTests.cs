using FluentAssertions;
using MealPlanner.Application.Meals;
using MealPlanner.Application.Meals.Validators;
using Xunit;

namespace MealPlanner.Application.Tests.Meals.Validators;

public sealed class SwapMealCommandValidatorTests
{
    private readonly SwapMealCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new SwapMealCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyMealId_ShouldFail()
    {
        // Arrange
        var command = new SwapMealCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MealId");
    }

    [Fact]
    public async Task Validate_WithEmptyNewRecipeId_ShouldFail()
    {
        // Arrange
        var command = new SwapMealCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewRecipeId");
    }

    [Fact]
    public async Task Validate_WithBothIdsEmpty_ShouldFailWithTwoErrors()
    {
        // Arrange
        var command = new SwapMealCommand(Guid.Empty, Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
