using FluentAssertions;
using MealPlanner.Application.Meals;
using MealPlanner.Application.Meals.Validators;
using Xunit;

namespace MealPlanner.Application.Tests.Meals.Validators;

public sealed class AddRecipeToMealPlanCommandValidatorTests
{
    private readonly AddRecipeToMealPlanCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new AddRecipeToMealPlanCommand(
            RecipeId: Guid.NewGuid(),
            Date: DateOnly.FromDateTime(DateTime.Today),
            MealType: "Lunch"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyRecipeId_ShouldFail()
    {
        // Arrange
        var command = new AddRecipeToMealPlanCommand(
            RecipeId: Guid.Empty,
            Date: DateOnly.FromDateTime(DateTime.Today),
            MealType: "Lunch"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RecipeId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyMealType_ShouldFail(string? mealType)
    {
        // Arrange
        var command = new AddRecipeToMealPlanCommand(
            RecipeId: Guid.NewGuid(),
            Date: DateOnly.FromDateTime(DateTime.Today),
            MealType: mealType!
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MealType");
    }

    [Fact]
    public async Task Validate_WithInvalidMealType_ShouldFail()
    {
        // Arrange
        var command = new AddRecipeToMealPlanCommand(
            RecipeId: Guid.NewGuid(),
            Date: DateOnly.FromDateTime(DateTime.Today),
            MealType: "InvalidMealType"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MealType");
    }

    [Theory]
    [InlineData("Breakfast")]
    [InlineData("Lunch")]
    [InlineData("Dinner")]
    [InlineData("breakfast")]
    [InlineData("LUNCH")]
    public async Task Validate_WithValidMealType_ShouldPass(string mealType)
    {
        // Arrange
        var command = new AddRecipeToMealPlanCommand(
            RecipeId: Guid.NewGuid(),
            Date: DateOnly.FromDateTime(DateTime.Today),
            MealType: mealType
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
