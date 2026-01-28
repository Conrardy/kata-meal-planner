using FluentAssertions;
using MealPlanner.Application.Meals;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.ShoppingList;
using Xunit;

namespace MealPlanner.Application.Tests.Meals.Handlers;

public sealed class AddRecipeToMealPlanCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithNewMeal_ShouldCreatePlannedMeal()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var syncService = new StubShoppingListSyncService();

        var handler = new AddRecipeToMealPlanCommandHandler(mealRepository, syncService);
        var command = new AddRecipeToMealPlanCommand(recipeId, date, "Dinner");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RecipeName.Should().Be("Recipe");
        result.Date.Should().Be(date.ToString("yyyy-MM-dd"));
        result.MealType.Should().Be("Dinner");
        result.ShoppingListUpdated.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithExistingMeal_ShouldSwapRecipe()
    {
        // Arrange
        var oldRecipeId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);
        var mealId = Guid.NewGuid();

        var mealRepository = new InMemoryPlannedMealRepository();
        var existingMeal = new PlannedMeal(mealId, date, MealType.Dinner, oldRecipeId);
        mealRepository.AddMeal(existingMeal);

        var syncService = new StubShoppingListSyncService();
        var handler = new AddRecipeToMealPlanCommandHandler(mealRepository, syncService);
        var command = new AddRecipeToMealPlanCommand(newRecipeId, date, "Dinner");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MealId.Should().Be(mealId);
        result.ShoppingListUpdated.Should().BeTrue();

        var updatedMeal = await mealRepository.GetByIdAsync(mealId);
        updatedMeal!.RecipeId.Should().Be(newRecipeId);
    }

    [Fact]
    public async Task Handle_ShouldMarkPendingSync()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now.AddDays(5));

        var mealRepository = new InMemoryPlannedMealRepository();
        var syncService = new StubShoppingListSyncService();

        var handler = new AddRecipeToMealPlanCommandHandler(mealRepository, syncService);
        var command = new AddRecipeToMealPlanCommand(recipeId, date, "Breakfast");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        syncService.LastSyncedDate.Should().Be(date);
    }

    [Fact]
    public async Task Handle_WithBreakfastMealType_ShouldCreateBreakfastMeal()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var syncService = new StubShoppingListSyncService();

        var handler = new AddRecipeToMealPlanCommandHandler(mealRepository, syncService);
        var command = new AddRecipeToMealPlanCommand(recipeId, date, "Breakfast");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.MealType.Should().Be("Breakfast");

        var meals = await mealRepository.GetByDateAsync(date);
        meals.Should().HaveCount(1);
        meals[0].MealType.Should().Be(MealType.Breakfast);
    }

    [Fact]
    public async Task Handle_WithLunchMealType_ShouldCreateLunchMeal()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var syncService = new StubShoppingListSyncService();

        var handler = new AddRecipeToMealPlanCommandHandler(mealRepository, syncService);
        var command = new AddRecipeToMealPlanCommand(recipeId, date, "Lunch");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.MealType.Should().Be("Lunch");

        var meals = await mealRepository.GetByDateAsync(date);
        meals.Should().HaveCount(1);
        meals[0].MealType.Should().Be(MealType.Lunch);
    }

    [Fact]
    public async Task Handle_AddingMultipleMealsForSameDate_ShouldCreateEach()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var syncService = new StubShoppingListSyncService();
        var handler = new AddRecipeToMealPlanCommandHandler(mealRepository, syncService);

        // Act
        await handler.Handle(new AddRecipeToMealPlanCommand(Guid.NewGuid(), date, "Breakfast"), CancellationToken.None);
        await handler.Handle(new AddRecipeToMealPlanCommand(Guid.NewGuid(), date, "Lunch"), CancellationToken.None);
        await handler.Handle(new AddRecipeToMealPlanCommand(Guid.NewGuid(), date, "Dinner"), CancellationToken.None);

        // Assert
        var meals = await mealRepository.GetByDateAsync(date);
        meals.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ReplacingExistingMeal_ShouldNotCreateDuplicate()
    {
        // Arrange
        var oldRecipeId = Guid.NewGuid();
        var newRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var existingMeal = new PlannedMeal(Guid.NewGuid(), date, MealType.Dinner, oldRecipeId);
        mealRepository.AddMeal(existingMeal);

        var syncService = new StubShoppingListSyncService();
        var handler = new AddRecipeToMealPlanCommandHandler(mealRepository, syncService);
        var command = new AddRecipeToMealPlanCommand(newRecipeId, date, "Dinner");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var meals = await mealRepository.GetByDateAsync(date);
        meals.Should().HaveCount(1);
        meals[0].RecipeId.Should().Be(newRecipeId);
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
