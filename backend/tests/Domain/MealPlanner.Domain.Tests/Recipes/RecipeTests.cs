using FluentAssertions;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Domain.Tests.Recipes;

public sealed class RecipeTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateRecipe()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Pasta Carbonara";

        // Act
        var recipe = new Recipe(id, name);

        // Assert
        recipe.Id.Should().Be(id);
        recipe.Name.Should().Be(name);
        recipe.ImageUrl.Should().BeNull();
        recipe.Description.Should().BeNull();
        recipe.Tags.Should().BeEmpty();
        recipe.MealType.Should().Be(MealType.Dinner);
        recipe.Ingredients.Should().BeEmpty();
        recipe.Steps.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithAllProperties_ShouldSetAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Grilled Salmon";
        var imageUrl = "https://example.com/salmon.jpg";
        var description = "Delicious grilled salmon with herbs";
        var tags = new List<string> { "Quick & Easy", "Low Carb" };
        var mealType = MealType.Dinner;
        var ingredients = new List<Ingredient>
        {
            new("Salmon fillet", "2", "pieces"),
            new("Olive oil", "2", "tbsp")
        };
        var steps = new List<CookingStep>
        {
            new(1, "Preheat grill"),
            new(2, "Season salmon"),
            new(3, "Grill for 5 minutes each side")
        };

        // Act
        var recipe = new Recipe(id, name, imageUrl, description, tags, mealType, ingredients, steps);

        // Assert
        recipe.Id.Should().Be(id);
        recipe.Name.Should().Be(name);
        recipe.ImageUrl.Should().Be(imageUrl);
        recipe.Description.Should().Be(description);
        recipe.Tags.Should().BeEquivalentTo(tags);
        recipe.MealType.Should().Be(mealType);
        recipe.Ingredients.Should().HaveCount(2);
        recipe.Steps.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var action = () => new Recipe(id, invalidName!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNullTags_ShouldDefaultToEmptyList()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var recipe = new Recipe(id, "Test Recipe", tags: null);

        // Assert
        recipe.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullMealType_ShouldDefaultToDinner()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var recipe = new Recipe(id, "Test Recipe", mealType: null);

        // Assert
        recipe.MealType.Should().Be(MealType.Dinner);
    }

    [Fact]
    public void Constructor_WithBreakfastMealType_ShouldSetBreakfast()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var recipe = new Recipe(id, "Pancakes", mealType: MealType.Breakfast);

        // Assert
        recipe.MealType.Should().Be(MealType.Breakfast);
    }

    [Fact]
    public void Constructor_WithLunchMealType_ShouldSetLunch()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var recipe = new Recipe(id, "Caesar Salad", mealType: MealType.Lunch);

        // Assert
        recipe.MealType.Should().Be(MealType.Lunch);
    }
}

public sealed class IngredientTests
{
    [Fact]
    public void Constructor_WithAllProperties_ShouldCreateIngredient()
    {
        // Arrange & Act
        var ingredient = new Ingredient("Flour", "2", "cups");

        // Assert
        ingredient.Name.Should().Be("Flour");
        ingredient.Quantity.Should().Be("2");
        ingredient.Unit.Should().Be("cups");
    }

    [Fact]
    public void Constructor_WithNullUnit_ShouldBeNull()
    {
        // Arrange & Act
        var ingredient = new Ingredient("Salt", "1", null);

        // Assert
        ingredient.Name.Should().Be("Salt");
        ingredient.Unit.Should().BeNull();
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var ingredient1 = new Ingredient("Sugar", "1", "cup");
        var ingredient2 = new Ingredient("Sugar", "1", "cup");

        // Assert
        ingredient1.Should().Be(ingredient2);
    }
}

public sealed class CookingStepTests
{
    [Fact]
    public void Constructor_ShouldCreateCookingStep()
    {
        // Arrange & Act
        var step = new CookingStep(1, "Preheat oven to 350F");

        // Assert
        step.StepNumber.Should().Be(1);
        step.Instruction.Should().Be("Preheat oven to 350F");
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var step1 = new CookingStep(1, "Mix ingredients");
        var step2 = new CookingStep(1, "Mix ingredients");

        // Assert
        step1.Should().Be(step2);
    }
}
