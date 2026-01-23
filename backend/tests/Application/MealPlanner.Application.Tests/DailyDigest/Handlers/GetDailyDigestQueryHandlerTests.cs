using FluentAssertions;
using MealPlanner.Application.DailyDigest;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Application.Tests.DailyDigest.Handlers;

public sealed class GetDailyDigestQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithMealsForDate_ShouldReturnDailyDigestDto()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryPlannedMealRepository();
        var recipe = new Recipe(Guid.NewGuid(), "Test Recipe", "https://example.com/image.jpg");

        var meal = new PlannedMeal(Guid.NewGuid(), date, MealType.Breakfast, recipe.Id);
        repository.AddMealWithRecipe(meal, recipe);

        var handler = new GetDailyDigestQueryHandler(repository);
        var query = new GetDailyDigestQuery(date);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.Meals.Should().HaveCount(1);
        result.Meals[0].RecipeName.Should().Be("Test Recipe");
        result.Meals[0].MealType.Should().Be("Breakfast");
    }

    [Fact]
    public async Task Handle_WithNoMealsForDate_ShouldReturnEmptyList()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryPlannedMealRepository();
        var handler = new GetDailyDigestQueryHandler(repository);
        var query = new GetDailyDigestQuery(date);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.Meals.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMultipleMeals_ShouldOrderByMealType()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryPlannedMealRepository();

        var dinnerRecipe = new Recipe(Guid.NewGuid(), "Dinner Recipe");
        var breakfastRecipe = new Recipe(Guid.NewGuid(), "Breakfast Recipe");
        var lunchRecipe = new Recipe(Guid.NewGuid(), "Lunch Recipe");

        var dinner = new PlannedMeal(Guid.NewGuid(), date, MealType.Dinner, dinnerRecipe.Id);
        var breakfast = new PlannedMeal(Guid.NewGuid(), date, MealType.Breakfast, breakfastRecipe.Id);
        var lunch = new PlannedMeal(Guid.NewGuid(), date, MealType.Lunch, lunchRecipe.Id);

        repository.AddMealWithRecipe(dinner, dinnerRecipe);
        repository.AddMealWithRecipe(breakfast, breakfastRecipe);
        repository.AddMealWithRecipe(lunch, lunchRecipe);

        var handler = new GetDailyDigestQueryHandler(repository);
        var query = new GetDailyDigestQuery(date);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Meals.Should().HaveCount(3);
        result.Meals[0].MealType.Should().Be("Breakfast");
        result.Meals[1].MealType.Should().Be("Lunch");
        result.Meals[2].MealType.Should().Be("Dinner");
    }

    [Fact]
    public async Task Handle_WithMealWithoutRecipe_ShouldReturnDefaultRecipeName()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryPlannedMealRepository();
        var meal = new PlannedMeal(Guid.NewGuid(), date, MealType.Lunch, Guid.NewGuid());
        repository.AddMeal(meal);

        var handler = new GetDailyDigestQueryHandler(repository);
        var query = new GetDailyDigestQuery(date);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Meals.Should().HaveCount(1);
        result.Meals[0].RecipeName.Should().Be("No recipe assigned");
        result.Meals[0].ImageUrl.Should().BeNull();
    }

    private sealed class InMemoryPlannedMealRepository : IPlannedMealRepository
    {
        private readonly List<PlannedMeal> _meals = [];
        private readonly Dictionary<Guid, Recipe> _recipes = [];

        public void AddMeal(PlannedMeal meal)
        {
            _meals.Add(meal);
        }

        public void AddMealWithRecipe(PlannedMeal meal, Recipe recipe)
        {
            _recipes[recipe.Id] = recipe;
            var mealWithRecipe = CreateMealWithRecipe(meal, recipe);
            _meals.Add(mealWithRecipe);
        }

        private static PlannedMeal CreateMealWithRecipe(PlannedMeal meal, Recipe recipe)
        {
            var newMeal = new PlannedMeal(meal.Id, meal.Date, meal.MealType, meal.RecipeId);
            var recipeField = typeof(PlannedMeal).GetProperty("Recipe");
            recipeField?.SetValue(newMeal, recipe);
            return newMeal;
        }

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
}
