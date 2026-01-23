using FluentAssertions;
using MealPlanner.Application.WeeklyPlan;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Application.Tests.WeeklyPlan.Handlers;

public sealed class GetWeeklyPlanQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnWeeklyPlanWithCorrectDateRange()
    {
        // Arrange
        var repository = new InMemoryPlannedMealRepository();
        var handler = new GetWeeklyPlanQueryHandler(repository);
        var startDate = new DateOnly(2026, 1, 20);
        var query = new GetWeeklyPlanQuery(startDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(startDate.AddDays(6));
    }

    [Fact]
    public async Task Handle_ShouldReturn7Days()
    {
        // Arrange
        var repository = new InMemoryPlannedMealRepository();
        var handler = new GetWeeklyPlanQueryHandler(repository);
        var startDate = new DateOnly(2026, 1, 20);
        var query = new GetWeeklyPlanQuery(startDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Days.Should().HaveCount(7);
    }

    [Fact]
    public async Task Handle_WithNoMeals_ShouldReturnNullMeals()
    {
        // Arrange
        var repository = new InMemoryPlannedMealRepository();
        var handler = new GetWeeklyPlanQueryHandler(repository);
        var startDate = new DateOnly(2026, 1, 20);
        var query = new GetWeeklyPlanQuery(startDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Days[0].Breakfast.Should().BeNull();
        result.Days[0].Lunch.Should().BeNull();
        result.Days[0].Dinner.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMeals_ShouldMapMealsCorrectly()
    {
        // Arrange
        var repository = new InMemoryPlannedMealRepository();
        var startDate = new DateOnly(2026, 1, 20);
        var recipe = new Recipe(Guid.NewGuid(), "Pancakes", "https://example.com/pancakes.jpg");
        var meal = new PlannedMeal(Guid.NewGuid(), startDate, MealType.Breakfast, recipe.Id);
        repository.AddMealWithRecipe(meal, recipe);

        var handler = new GetWeeklyPlanQueryHandler(repository);
        var query = new GetWeeklyPlanQuery(startDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Days[0].Breakfast.Should().NotBeNull();
        result.Days[0].Breakfast!.RecipeName.Should().Be("Pancakes");
        result.Days[0].Breakfast.ImageUrl.Should().Be("https://example.com/pancakes.jpg");
    }

    [Fact]
    public async Task Handle_ShouldMapDayOfWeekCorrectly()
    {
        // Arrange
        var repository = new InMemoryPlannedMealRepository();
        var handler = new GetWeeklyPlanQueryHandler(repository);
        var startDate = new DateOnly(2026, 1, 20);
        var query = new GetWeeklyPlanQuery(startDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Days[0].DayName.Should().Be(startDate.DayOfWeek.ToString());
    }

    [Fact]
    public async Task Handle_WithMultipleMealTypes_ShouldMapEachMealType()
    {
        // Arrange
        var repository = new InMemoryPlannedMealRepository();
        var startDate = new DateOnly(2026, 1, 20);

        var breakfastRecipe = new Recipe(Guid.NewGuid(), "Breakfast Recipe");
        var lunchRecipe = new Recipe(Guid.NewGuid(), "Lunch Recipe");
        var dinnerRecipe = new Recipe(Guid.NewGuid(), "Dinner Recipe");

        var breakfast = new PlannedMeal(Guid.NewGuid(), startDate, MealType.Breakfast, breakfastRecipe.Id);
        var lunch = new PlannedMeal(Guid.NewGuid(), startDate, MealType.Lunch, lunchRecipe.Id);
        var dinner = new PlannedMeal(Guid.NewGuid(), startDate, MealType.Dinner, dinnerRecipe.Id);

        repository.AddMealWithRecipe(breakfast, breakfastRecipe);
        repository.AddMealWithRecipe(lunch, lunchRecipe);
        repository.AddMealWithRecipe(dinner, dinnerRecipe);

        var handler = new GetWeeklyPlanQueryHandler(repository);
        var query = new GetWeeklyPlanQuery(startDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Days[0].Breakfast!.RecipeName.Should().Be("Breakfast Recipe");
        result.Days[0].Lunch!.RecipeName.Should().Be("Lunch Recipe");
        result.Days[0].Dinner!.RecipeName.Should().Be("Dinner Recipe");
    }

    [Fact]
    public async Task Handle_WithMealWithoutRecipe_ShouldShowDefaultRecipeName()
    {
        // Arrange
        var repository = new InMemoryPlannedMealRepository();
        var startDate = new DateOnly(2026, 1, 20);
        var meal = new PlannedMeal(Guid.NewGuid(), startDate, MealType.Lunch, Guid.NewGuid());
        repository.AddMeal(meal);

        var handler = new GetWeeklyPlanQueryHandler(repository);
        var query = new GetWeeklyPlanQuery(startDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Days[0].Lunch!.RecipeName.Should().Be("No recipe assigned");
        result.Days[0].Lunch.ImageUrl.Should().BeNull();
    }

    private sealed class InMemoryPlannedMealRepository : IPlannedMealRepository
    {
        private readonly List<PlannedMeal> _meals = [];
        private readonly Dictionary<Guid, Recipe> _recipes = [];

        public void AddMeal(PlannedMeal meal) => _meals.Add(meal);

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
