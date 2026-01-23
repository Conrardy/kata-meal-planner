using FluentAssertions;
using MealPlanner.Application.Recipes;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Application.Tests.Recipes.Handlers;

public sealed class CreateRecipeCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRecipeAndReturnId()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var handler = new CreateRecipeCommandHandler(repository);
        var command = CreateValidCommand();

        // Act
        var recipeId = await handler.Handle(command, CancellationToken.None);

        // Assert
        recipeId.Should().NotBeEmpty();
        repository.Recipes.Should().HaveCount(1);
        var createdRecipe = repository.Recipes.First();
        createdRecipe.Id.Should().Be(recipeId);
        createdRecipe.Name.Should().Be(command.Name);
        createdRecipe.Description.Should().Be(command.Description);
        createdRecipe.ImageUrl.Should().Be(command.ImageUrl);
        createdRecipe.Ingredients.Should().HaveCount(1);
        createdRecipe.Steps.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNullOptionalFields_ShouldCreateRecipeWithDefaults()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var handler = new CreateRecipeCommandHandler(repository);
        var command = new CreateRecipeCommand(
            Name: "Simple Recipe",
            ImageUrl: null,
            Description: null,
            Ingredients: [new CreateIngredientDto("Flour", "2", "cups")],
            Steps: [new CreateCookingStepDto(1, "Mix")],
            Tags: null,
            MealType: null
        );

        // Act
        var recipeId = await handler.Handle(command, CancellationToken.None);

        // Assert
        recipeId.Should().NotBeEmpty();
        var createdRecipe = repository.Recipes.First();
        createdRecipe.ImageUrl.Should().BeNull();
        createdRecipe.Description.Should().BeNull();
        createdRecipe.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMealType_ShouldSetCorrectMealType()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var handler = new CreateRecipeCommandHandler(repository);
        var command = CreateValidCommand() with { MealType = "Breakfast" };

        // Act
        var recipeId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdRecipe = repository.Recipes.First();
        createdRecipe.MealType.Value.Should().Be("Breakfast");
    }

    [Fact]
    public async Task Handle_WithMultipleIngredients_ShouldCreateAllIngredients()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var handler = new CreateRecipeCommandHandler(repository);
        var command = CreateValidCommand() with
        {
            Ingredients =
            [
                new CreateIngredientDto("Flour", "2", "cups"),
                new CreateIngredientDto("Sugar", "1", "cup"),
                new CreateIngredientDto("Salt", "1", "pinch")
            ]
        };

        // Act
        var recipeId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdRecipe = repository.Recipes.First();
        createdRecipe.Ingredients.Should().HaveCount(3);
        createdRecipe.Ingredients.Select(i => i.Name).Should().Contain(["Flour", "Sugar", "Salt"]);
    }

    [Fact]
    public async Task Handle_WithMultipleSteps_ShouldCreateAllSteps()
    {
        // Arrange
        var repository = new InMemoryRecipeRepository();
        var handler = new CreateRecipeCommandHandler(repository);
        var command = CreateValidCommand() with
        {
            Steps =
            [
                new CreateCookingStepDto(1, "Preheat oven"),
                new CreateCookingStepDto(2, "Mix ingredients"),
                new CreateCookingStepDto(3, "Bake")
            ]
        };

        // Act
        var recipeId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdRecipe = repository.Recipes.First();
        createdRecipe.Steps.Should().HaveCount(3);
        createdRecipe.Steps.Select(s => s.StepNumber).Should().BeInAscendingOrder();
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

    private sealed class InMemoryRecipeRepository : IRecipeRepository
    {
        public List<Recipe> Recipes { get; } = [];

        public Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Recipe>>(Recipes.AsReadOnly());

        public Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Recipes.FirstOrDefault(r => r.Id == id));

        public Task<IReadOnlyList<Recipe>> GetSuggestionsAsync(Guid excludeRecipeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Recipe>>(Recipes.Where(r => r.Id != excludeRecipeId).ToList().AsReadOnly());

        public Task<IReadOnlyList<Recipe>> SearchAsync(string? searchTerm, IReadOnlyList<string>? tags, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Recipe>>(Recipes.AsReadOnly());

        public Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(Recipes.SelectMany(r => r.Tags).Distinct().ToList().AsReadOnly());

        public Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default)
        {
            Recipes.Add(recipe);
            return Task.CompletedTask;
        }
    }
}
