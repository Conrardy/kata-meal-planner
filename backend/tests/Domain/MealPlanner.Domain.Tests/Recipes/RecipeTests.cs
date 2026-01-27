using FluentAssertions;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Domain.Tests.Recipes;

public sealed class RecipeTests
{
    [Fact]
    public void Constructor_WithValidNameIngredientsAndSteps_ShouldCreateRecipe()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Pasta Carbonara";
        var ingredients = new List<Ingredient> { new("Pasta", "200", "g") };
        var steps = new List<CookingStep> { new(1, "Cook pasta") };

        // Act
        var recipe = new Recipe(id, name, ingredients: ingredients, steps: steps);

        // Assert
        recipe.Id.Should().Be(id);
        recipe.Name.Should().Be(name);
        recipe.ImageUrl.Should().BeNull();
        recipe.Description.Should().BeNull();
        recipe.Tags.Should().BeEmpty();
        recipe.MealType.Should().Be(MealType.Dinner);
        recipe.Ingredients.Should().HaveCount(1);
        recipe.Steps.Should().HaveCount(1);
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
        var ingredients = new List<Ingredient> { new("Test", "1", "unit") };
        var steps = new List<CookingStep> { new(1, "Test step") };

        // Act
        var recipe = new Recipe(id, "Test Recipe", tags: null, ingredients: ingredients, steps: steps);

        // Assert
        recipe.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullMealType_ShouldDefaultToDinner()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = new List<Ingredient> { new("Test", "1", "unit") };
        var steps = new List<CookingStep> { new(1, "Test step") };

        // Act
        var recipe = new Recipe(id, "Test Recipe", mealType: null, ingredients: ingredients, steps: steps);

        // Assert
        recipe.MealType.Should().Be(MealType.Dinner);
    }

    [Fact]
    public void Constructor_WithBreakfastMealType_ShouldSetBreakfast()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = new List<Ingredient> { new("Eggs", "2", "pieces") };
        var steps = new List<CookingStep> { new(1, "Scramble eggs") };

        // Act
        var recipe = new Recipe(id, "Pancakes", mealType: MealType.Breakfast, ingredients: ingredients, steps: steps);

        // Assert
        recipe.MealType.Should().Be(MealType.Breakfast);
    }

    [Fact]
    public void Constructor_WithLunchMealType_ShouldSetLunch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = new List<Ingredient> { new("Lettuce", "1", "head") };
        var steps = new List<CookingStep> { new(1, "Prepare salad") };

        // Act
        var recipe = new Recipe(id, "Caesar Salad", mealType: MealType.Lunch, ingredients: ingredients, steps: steps);

        // Assert
        recipe.MealType.Should().Be(MealType.Lunch);
    }

    [Fact]
    public void Constructor_WithEmptyIngredients_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var emptyIngredients = new List<Ingredient>();
        var steps = new List<CookingStep> { new(1, "Test step") };

        // Act
        var action = () => new Recipe(id, "Test Recipe", ingredients: emptyIngredients, steps: steps);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe must have at least one ingredient*");
    }

    [Fact]
    public void Constructor_WithNullIngredients_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var steps = new List<CookingStep> { new(1, "Test step") };

        // Act
        var action = () => new Recipe(id, "Test Recipe", ingredients: null, steps: steps);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe must have at least one ingredient*");
    }

    [Fact]
    public void Constructor_WithEmptySteps_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = new List<Ingredient> { new("Test", "1", "unit") };
        var emptySteps = new List<CookingStep>();

        // Act
        var action = () => new Recipe(id, "Test Recipe", ingredients: ingredients, steps: emptySteps);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe must have at least one step*");
    }

    [Fact]
    public void Constructor_WithNullSteps_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = new List<Ingredient> { new("Test", "1", "unit") };

        // Act
        var action = () => new Recipe(id, "Test Recipe", ingredients: ingredients, steps: null);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Recipe must have at least one step*");
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
