using FluentAssertions;
using MealPlanner.Application.Recipes;
using MealPlanner.Application.Recipes.Validators;
using Xunit;

namespace MealPlanner.Application.Tests.Recipes.Validators;

public sealed class CreateRecipeCommandValidatorTests
{
    private readonly CreateRecipeCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyName_ShouldFail(string? name)
    {
        // Arrange
        var command = CreateValidCommand() with { Name = name! };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with { Name = new string('a', 201) };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public async Task Validate_WithInvalidImageUrl_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with { ImageUrl = "not-a-valid-url" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ImageUrl");
    }

    [Theory]
    [InlineData("https://example.com/image.jpg")]
    [InlineData("http://example.com/image.png")]
    [InlineData(null)]
    public async Task Validate_WithValidImageUrl_ShouldPass(string? imageUrl)
    {
        // Arrange
        var command = CreateValidCommand() with { ImageUrl = imageUrl };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyIngredients_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with { Ingredients = [] };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Ingredients");
    }

    [Fact]
    public async Task Validate_WithEmptySteps_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with { Steps = [] };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Steps");
    }

    [Fact]
    public async Task Validate_WithInvalidMealType_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with { MealType = "InvalidMealType" };

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
    [InlineData(null)]
    public async Task Validate_WithValidMealType_ShouldPass(string? mealType)
    {
        // Arrange
        var command = CreateValidCommand() with { MealType = mealType };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithTooManyTags_ShouldFail()
    {
        // Arrange
        var tags = Enumerable.Range(1, 21).Select(i => $"tag{i}").ToList();
        var command = CreateValidCommand() with { Tags = tags };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Tags");
    }

    [Fact]
    public async Task Validate_WithInvalidIngredient_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Ingredients = [new CreateIngredientDto("", "1", "cup")]
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Ingredients"));
    }

    [Fact]
    public async Task Validate_WithInvalidStep_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Steps = [new CreateCookingStepDto(0, "Mix ingredients")]
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Steps"));
    }

    private static CreateRecipeCommand CreateValidCommand()
    {
        return new CreateRecipeCommand(
            Name: "Test Recipe",
            ImageUrl: "https://example.com/image.jpg",
            Description: "A test recipe",
            Ingredients: [new CreateIngredientDto("Flour", "2", "cups")],
            Steps: [new CreateCookingStepDto(1, "Mix all ingredients")],
            Tags: ["Quick & Easy"],
            MealType: "Dinner"
        );
    }
}
