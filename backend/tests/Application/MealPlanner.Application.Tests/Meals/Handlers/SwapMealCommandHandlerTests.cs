using ErrorOr;
using FluentAssertions;
using MealPlanner.Application.Meals;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.ShoppingList;
using Xunit;

namespace MealPlanner.Application.Tests.Meals.Handlers;

public sealed class SwapMealCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidMealAndRecipe_ShouldSwapRecipe()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var originalRecipeId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var syncService = new StubShoppingListSyncService();

        var originalRecipe = new Recipe(originalRecipeId, "Original Recipe",
            ingredients: new List<Ingredient> { new("Test", "1", "unit") },
            steps: new List<CookingStep> { new(1, "Test step") });
        var newRecipe = new Recipe(newRecipeId, "New Recipe", "https://example.com/new.jpg",
            ingredients: new List<Ingredient> { new("Test", "1", "unit") },
            steps: new List<CookingStep> { new(1, "Test step") });
        var meal = new PlannedMeal(mealId, date, MealType.Dinner, originalRecipeId);

        recipeRepository.AddRecipe(originalRecipe);
        recipeRepository.AddRecipe(newRecipe);
        mealRepository.AddMeal(meal);

        var handler = new SwapMealCommandHandler(mealRepository, recipeRepository, syncService);
        var command = new SwapMealCommand(mealId, newRecipeId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.MealId.Should().Be(mealId);
        result.Value.RecipeName.Should().Be("New Recipe");
        result.Value.ImageUrl.Should().Be("https://example.com/new.jpg");
        result.Value.ShoppingListUpdated.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenMealNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var syncService = new StubShoppingListSyncService();

        var handler = new SwapMealCommandHandler(mealRepository, recipeRepository, syncService);
        var command = new SwapMealCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Meal.NotFound");
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var syncService = new StubShoppingListSyncService();

        var meal = new PlannedMeal(mealId, date, MealType.Dinner, Guid.NewGuid());
        mealRepository.AddMeal(meal);

        var handler = new SwapMealCommandHandler(mealRepository, recipeRepository, syncService);
        var command = new SwapMealCommand(mealId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Meal.RecipeNotFound");
    }

    [Fact]
    public async Task Handle_ShouldMarkPendingSyncWithCorrectDate()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now.AddDays(3));

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var syncService = new StubShoppingListSyncService();

        var newRecipe = new Recipe(newRecipeId, "New Recipe",
            ingredients: new List<Ingredient> { new("Test", "1", "unit") },
            steps: new List<CookingStep> { new(1, "Test step") });
        var meal = new PlannedMeal(mealId, date, MealType.Lunch, Guid.NewGuid());

        recipeRepository.AddRecipe(newRecipe);
        mealRepository.AddMeal(meal);

        var handler = new SwapMealCommandHandler(mealRepository, recipeRepository, syncService);
        var command = new SwapMealCommand(mealId, newRecipeId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        syncService.LastSyncedDate.Should().Be(date);
    }

    [Fact]
    public async Task Handle_ShouldUpdateMealInRepository()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var originalRecipeId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var syncService = new StubShoppingListSyncService();

        var newRecipe = new Recipe(newRecipeId, "New Recipe",
            ingredients: new List<Ingredient> { new("Test", "1", "unit") },
            steps: new List<CookingStep> { new(1, "Test step") });
        var meal = new PlannedMeal(mealId, date, MealType.Dinner, originalRecipeId);

        recipeRepository.AddRecipe(newRecipe);
        mealRepository.AddMeal(meal);

        var handler = new SwapMealCommandHandler(mealRepository, recipeRepository, syncService);
        var command = new SwapMealCommand(mealId, newRecipeId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedMeal = await mealRepository.GetByIdAsync(mealId);
        updatedMeal!.RecipeId.Should().Be(newRecipeId);
    }

    private sealed class InMemoryPlannedMealRepository : IPlannedMealRepository
    {
        private readonly List<PlannedMeal> _meals = [];

        public void AddMeal(PlannedMeal meal) => _meals.Add(meal);

        public Task<IReadOnlyList<PlannedMeal>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PlannedMeal>>(_meals.Where(m => m.Date == date).ToList().AsReadOnly());

        public Task<IReadOnlyList<PlannedMeal>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PlannedMeal>>(_meals.Where(m => m.Date >= startDate && m.Date <= endDate).ToList().AsReadOnly());

        public Task<PlannedMeal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_meals.FirstOrDefault(m => m.Id == id));

        public Task<PlannedMeal?> GetByDateAndMealTypeAsync(DateOnly date, MealType mealType, CancellationToken cancellationToken = default)
            => Task.FromResult(_meals.FirstOrDefault(m => m.Date == date && m.MealType == mealType));

        public Task UpdateAsync(PlannedMeal meal, CancellationToken cancellationToken = default)
        {
            var index = _meals.FindIndex(m => m.Id == meal.Id);
            if (index >= 0) _meals[index] = meal;
            return Task.CompletedTask;
        }

        public Task AddAsync(PlannedMeal meal, CancellationToken cancellationToken = default)
        {
            _meals.Add(meal);
            return Task.CompletedTask;
        }
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

    private sealed class StubShoppingListSyncService : IShoppingListSyncService
    {
        public DateOnly? LastSyncedDate { get; private set; }

        public void MarkPendingSync(DateOnly affectedDate) => LastSyncedDate = affectedDate;
        public DateTime? GetLastSyncTimestamp(DateOnly startDate) => null;
        public void MarkSynced(DateOnly startDate) { }
        public bool HasPendingChanges(DateOnly startDate) => false;
        public int GetPendingChangeCount(DateOnly startDate) => 0;
    }
}
