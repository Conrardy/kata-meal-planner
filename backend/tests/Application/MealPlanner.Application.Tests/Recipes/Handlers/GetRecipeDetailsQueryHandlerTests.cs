using FluentAssertions;
using MealPlanner.Application.Recipes;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Application.Tests.Recipes.Handlers;

public sealed class GetRecipeDetailsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingRecipe_ShouldReturnRecipeDetails()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var recipeId = Guid.NewGuid();
        var ingredients = new List<Ingredient>
        {
            new("Flour", "2", "cups"),
            new("Sugar", "1", "cup")
        };
        var steps = new List<CookingStep>
        {
            new(1, "Mix dry ingredients"),
            new(2, "Add wet ingredients"),
            new(3, "Bake for 30 minutes")
        };
        var recipe = new Recipe(
            recipeId,
            "Chocolate Cake",
            "https://example.com/cake.jpg",
            "Delicious chocolate cake",
            ["Desserts"],
            MealType.Dinner,
            ingredients,
            steps
        );
        repository.AddRecipe(recipe);

        var handler = new GetRecipeDetailsQueryHandler(repository);
        var query = new GetRecipeDetailsQuery(recipeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(recipeId);
        result.Name.Should().Be("Chocolate Cake");
        result.ImageUrl.Should().Be("https://example.com/cake.jpg");
        result.Description.Should().Be("Delicious chocolate cake");
        result.Tags.Should().BeEquivalentTo(["Desserts"]);
        result.MealType.Should().Be("Dinner");
    }

    [Fact]
    public async Task Handle_WithExistingRecipe_ShouldReturnIngredients()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var recipeId = Guid.NewGuid();
        var ingredients = new List<Ingredient>
        {
            new("Flour", "2", "cups"),
            new("Salt", "1", null)
        };
        var recipe = new Recipe(recipeId, "Test Recipe", ingredients: ingredients);
        repository.AddRecipe(recipe);

        var handler = new GetRecipeDetailsQueryHandler(repository);
        var query = new GetRecipeDetailsQuery(recipeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result!.Ingredients.Should().HaveCount(2);
        result.Ingredients[0].Name.Should().Be("Flour");
        result.Ingredients[0].Quantity.Should().Be("2");
        result.Ingredients[0].Unit.Should().Be("cups");
        result.Ingredients[1].Name.Should().Be("Salt");
        result.Ingredients[1].Unit.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithExistingRecipe_ShouldReturnSteps()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var recipeId = Guid.NewGuid();
        var steps = new List<CookingStep>
        {
            new(1, "First step"),
            new(2, "Second step")
        };
        var recipe = new Recipe(recipeId, "Test Recipe", steps: steps);
        repository.AddRecipe(recipe);

        var handler = new GetRecipeDetailsQueryHandler(repository);
        var query = new GetRecipeDetailsQuery(recipeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result!.Steps.Should().HaveCount(2);
        result.Steps[0].StepNumber.Should().Be(1);
        result.Steps[0].Instruction.Should().Be("First step");
        result.Steps[1].StepNumber.Should().Be(2);
        result.Steps[1].Instruction.Should().Be("Second step");
    }

    [Fact]
    public async Task Handle_WithNonExistentRecipe_ShouldReturnNull()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var handler = new GetRecipeDetailsQueryHandler(repository);
        var query = new GetRecipeDetailsQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithBreakfastMealType_ShouldReturnCorrectMealType()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var recipeId = Guid.NewGuid();
        var recipe = new Recipe(recipeId, "Pancakes", mealType: MealType.Breakfast);
        repository.AddRecipe(recipe);

        var handler = new GetRecipeDetailsQueryHandler(repository);
        var query = new GetRecipeDetailsQuery(recipeId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result!.MealType.Should().Be("Breakfast");
    }

    private sealed class InMemoryRecipeRepository : IRecipeRepository
    {
        private readonly List<Recipe> _recipes = [];

        public void AddRecipe(Recipe recipe) => _recipes.Add(recipe);

        public Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Recipe>>(_recipes.AsReadOnly());

        public Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_recipes.FirstOrDefault(r => r.Id == id));

        public Task<IReadOnlyList<Recipe>> GetSuggestionsAsync(Guid excludeRecipeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Recipe>>(_recipes.Where(r => r.Id != excludeRecipeId).ToList().AsReadOnly());

        public Task<IReadOnlyList<Recipe>> SearchAsync(string? searchTerm, IReadOnlyList<string>? tags, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Recipe>>(_recipes.AsReadOnly());

        public Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(_recipes.SelectMany(r => r.Tags).Distinct().ToList().AsReadOnly());

        public Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default)
        {
            _recipes.Add(recipe);
            return Task.CompletedTask;
        }
    }
}
