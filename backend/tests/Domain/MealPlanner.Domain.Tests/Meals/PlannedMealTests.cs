using FluentAssertions;
using MealPlanner.Domain.Meals;
using Xunit;

namespace MealPlanner.Domain.Tests.Meals;

public sealed class PlannedMealTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePlannedMeal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var mealType = MealType.Breakfast;
        var recipeId = Guid.NewGuid();

        // Act
        var plannedMeal = new PlannedMeal(id, date, mealType, recipeId);

        // Assert
        plannedMeal.Id.Should().Be(id);
        plannedMeal.Date.Should().Be(date);
        plannedMeal.MealType.Should().Be(mealType);
        plannedMeal.RecipeId.Should().Be(recipeId);
        plannedMeal.Recipe.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithLunchMealType_ShouldSetLunch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var mealType = MealType.Lunch;
        var recipeId = Guid.NewGuid();

        // Act
        var plannedMeal = new PlannedMeal(id, date, mealType, recipeId);

        // Assert
        plannedMeal.MealType.Should().Be(MealType.Lunch);
    }

    [Fact]
    public void Constructor_WithDinnerMealType_ShouldSetDinner()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var mealType = MealType.Dinner;
        var recipeId = Guid.NewGuid();

        // Act
        var plannedMeal = new PlannedMeal(id, date, mealType, recipeId);

        // Assert
        plannedMeal.MealType.Should().Be(MealType.Dinner);
    }

    [Fact]
    public void SwapRecipe_ShouldUpdateRecipeId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var originalRecipeId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var plannedMeal = new PlannedMeal(id, date, MealType.Dinner, originalRecipeId);

        // Act
        plannedMeal.SwapRecipe(newRecipeId);

        // Assert
        plannedMeal.RecipeId.Should().Be(newRecipeId);
    }

    [Fact]
    public void SwapRecipe_ShouldClearRecipeNavigation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var originalRecipeId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var plannedMeal = new PlannedMeal(id, date, MealType.Dinner, originalRecipeId);

        // Act
        plannedMeal.SwapRecipe(newRecipeId);

        // Assert
        plannedMeal.Recipe.Should().BeNull();
    }

    [Fact]
    public void SwapRecipe_WithSameRecipeId_ShouldStillUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var recipeId = Guid.NewGuid();
        var plannedMeal = new PlannedMeal(id, date, MealType.Dinner, recipeId);

        // Act
        plannedMeal.SwapRecipe(recipeId);

        // Assert
        plannedMeal.RecipeId.Should().Be(recipeId);
        plannedMeal.Recipe.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFutureDate_ShouldCreatePlannedMeal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));
        var recipeId = Guid.NewGuid();

        // Act
        var plannedMeal = new PlannedMeal(id, futureDate, MealType.Breakfast, recipeId);

        // Assert
        plannedMeal.Date.Should().Be(futureDate);
    }

    [Fact]
    public void Constructor_WithPastDate_ShouldCreatePlannedMeal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
        var recipeId = Guid.NewGuid();

        // Act
        var plannedMeal = new PlannedMeal(id, pastDate, MealType.Lunch, recipeId);

        // Assert
        plannedMeal.Date.Should().Be(pastDate);
    }
}

public sealed class MealTypeTests
{
    [Fact]
    public void Breakfast_ShouldHaveCorrectValue()
    {
        MealType.Breakfast.Value.Should().Be("Breakfast");
    }

    [Fact]
    public void Lunch_ShouldHaveCorrectValue()
    {
        MealType.Lunch.Value.Should().Be("Lunch");
    }

    [Fact]
    public void Dinner_ShouldHaveCorrectValue()
    {
        MealType.Dinner.Value.Should().Be("Dinner");
    }

    [Theory]
    [InlineData("breakfast", "Breakfast")]
    [InlineData("Breakfast", "Breakfast")]
    [InlineData("BREAKFAST", "Breakfast")]
    [InlineData("lunch", "Lunch")]
    [InlineData("Lunch", "Lunch")]
    [InlineData("LUNCH", "Lunch")]
    [InlineData("dinner", "Dinner")]
    [InlineData("Dinner", "Dinner")]
    [InlineData("DINNER", "Dinner")]
    public void FromString_WithValidValue_ShouldReturnCorrectMealType(string input, string expected)
    {
        // Act
        var mealType = MealType.FromString(input);

        // Assert
        mealType.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("brunch")]
    [InlineData("snack")]
    [InlineData("")]
    [InlineData("meal")]
    public void FromString_WithInvalidValue_ShouldThrowArgumentException(string input)
    {
        // Act
        var action = () => MealType.FromString(input);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid meal type: {input}*");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        MealType.Breakfast.ToString().Should().Be("Breakfast");
        MealType.Lunch.ToString().Should().Be("Lunch");
        MealType.Dinner.ToString().Should().Be("Dinner");
    }

    [Fact]
    public void Equality_SameMealType_ShouldBeEqual()
    {
        // Arrange
        var mealType1 = MealType.FromString("breakfast");
        var mealType2 = MealType.Breakfast;

        // Assert
        mealType1.Should().Be(mealType2);
    }
}
