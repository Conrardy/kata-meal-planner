using FluentAssertions;
using MealPlanner.Application.Preferences;
using MealPlanner.Application.Preferences.Validators;
using Xunit;

namespace MealPlanner.Application.Tests.Preferences.Validators;

public sealed class UpdateUserPreferencesCommandValidatorTests
{
    private readonly UpdateUserPreferencesCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: ["Gluten", "Dairy"]
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyDietaryPreference_ShouldFail(string? preference)
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: preference!,
            Allergies: []
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DietaryPreference");
    }

    [Fact]
    public async Task Validate_WithInvalidDietaryPreference_ShouldFail()
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "InvalidPreference",
            Allergies: []
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DietaryPreference");
    }

    [Theory]
    [InlineData("None")]
    [InlineData("Vegetarian")]
    [InlineData("Vegan")]
    [InlineData("Pescatarian")]
    [InlineData("Keto")]
    [InlineData("Paleo")]
    [InlineData("Low Carb")]
    [InlineData("Mediterranean")]
    public async Task Validate_WithValidDietaryPreference_ShouldPass(string preference)
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: preference,
            Allergies: []
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidAllergy_ShouldFail()
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: ["InvalidAllergy"]
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Allergies"));
    }

    [Theory]
    [InlineData("Gluten")]
    [InlineData("Dairy")]
    [InlineData("Nuts")]
    [InlineData("Eggs")]
    [InlineData("Shellfish")]
    [InlineData("Soy")]
    public async Task Validate_WithValidAllergy_ShouldPass(string allergy)
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: [allergy]
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Validate_WithInvalidMealsPerDay_ShouldFail(int mealsPerDay)
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: [],
            MealsPerDay: mealsPerDay
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MealsPerDay");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Validate_WithValidMealsPerDay_ShouldPass(int mealsPerDay)
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: [],
            MealsPerDay: mealsPerDay
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithTooManyExcludedIngredients_ShouldFail()
    {
        // Arrange
        var excludedIngredients = Enumerable.Range(1, 51).Select(i => $"ingredient{i}").ToList();
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: [],
            ExcludedIngredients: excludedIngredients
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ExcludedIngredients");
    }
}
