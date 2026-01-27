using FluentAssertions;
using MealPlanner.Application.Recipes;
using MealPlanner.Application.Tests.TestHelpers;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Application.Tests.Recipes.Handlers;

public sealed class SearchRecipesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllRecipes()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        repository.AddRecipe(RecipeBuilder.Create().WithName("Pasta").WithTags("Quick & Easy").Build());
        repository.AddRecipe(RecipeBuilder.Create().WithName("Salad").WithTags("Healthy").Build());

        var handler = new SearchRecipesQueryHandler(repository);
        var query = new SearchRecipesQuery(null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Recipes.Should().HaveCount(2);
        result.AvailableTags.Should().Contain("Quick & Easy");
        result.AvailableTags.Should().Contain("Healthy");
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldReturnMatchingRecipes()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        repository.AddRecipe(RecipeBuilder.Create().WithName("Chicken Pasta").Build());
        repository.AddRecipe(RecipeBuilder.Create().WithName("Vegetable Salad").Build());

        var handler = new SearchRecipesQueryHandler(repository);
        var query = new SearchRecipesQuery("Chicken", null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recipes.Should().HaveCount(1);
        result.Recipes[0].Name.Should().Be("Chicken Pasta");
    }

    [Fact]
    public async Task Handle_WithTags_ShouldReturnFilteredRecipes()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        repository.AddRecipe(RecipeBuilder.Create().WithName("Quick Pasta").WithTags("Quick & Easy").Build());
        repository.AddRecipe(RecipeBuilder.Create().WithName("Complex Dish").WithTags("Gourmet").Build());

        var handler = new SearchRecipesQueryHandler(repository);
        var query = new SearchRecipesQuery(null, ["Quick & Easy"]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recipes.Should().HaveCount(1);
        result.Recipes[0].Name.Should().Be("Quick Pasta");
    }

    [Fact]
    public async Task Handle_WithNoMatchingRecipes_ShouldReturnEmptyList()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var handler = new SearchRecipesQueryHandler(repository);
        var query = new SearchRecipesQuery("NonExistent", null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recipes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectRecipeDetails()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var recipeId = Guid.NewGuid();
        repository.AddRecipe(RecipeBuilder.Create()
            .WithId(recipeId)
            .WithName("Test Recipe")
            .WithImageUrl("https://example.com/image.jpg")
            .WithDescription("A test description")
            .WithTags("Tag1", "Tag2")
            .Build());

        var handler = new SearchRecipesQueryHandler(repository);
        var query = new SearchRecipesQuery(null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Recipes.Should().HaveCount(1);
        result.Recipes[0].Id.Should().Be(recipeId);
        result.Recipes[0].Name.Should().Be("Test Recipe");
        result.Recipes[0].ImageUrl.Should().Be("https://example.com/image.jpg");
        result.Recipes[0].Description.Should().Be("A test description");
        result.Recipes[0].Tags.Should().BeEquivalentTo(["Tag1", "Tag2"]);
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
        {
            var result = _recipes.AsEnumerable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                result = result.Where(r => r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (tags is { Count: > 0 })
            {
                result = result.Where(r => r.Tags.Any(t => tags.Contains(t)));
            }

            return Task.FromResult<IReadOnlyList<Recipe>>(result.ToList().AsReadOnly());
        }

        public Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(_recipes.SelectMany(r => r.Tags).Distinct().ToList().AsReadOnly());

        public Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default)
        {
            _recipes.Add(recipe);
            return Task.CompletedTask;
        }
    }
}
