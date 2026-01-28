using ErrorOr;
using FluentAssertions;
using MealPlanner.Application.Meals;
using MealPlanner.Application.Tests.TestHelpers;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Preferences;
using MealPlanner.Domain.Recipes;
using Xunit;

namespace MealPlanner.Application.Tests.Meals.Handlers;

public sealed class GetSuggestionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithValidMealId_ShouldReturnSuggestions()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var currentRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var preferencesRepository = new InMemoryUserPreferencesRepository();
        var allergenDetector = new StubAllergenDetector();

        var currentRecipe = RecipeBuilder.Create().WithId(currentRecipeId).WithName("Current Recipe").Build();
        var suggestion1 = RecipeBuilder.Create().WithName("Suggestion 1").Build();
        var suggestion2 = RecipeBuilder.Create().WithName("Suggestion 2").Build();

        var meal = new PlannedMeal(mealId, date, MealType.Dinner, currentRecipeId);

        mealRepository.AddMeal(meal);
        recipeRepository.AddRecipe(currentRecipe);
        recipeRepository.AddRecipe(suggestion1);
        recipeRepository.AddRecipe(suggestion2);

        var handler = new GetSuggestionsQueryHandler(mealRepository, recipeRepository, preferencesRepository, allergenDetector);
        var query = new GetSuggestionsQuery(mealId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.MealId.Should().Be(mealId);
        result.Value.MealType.Should().Be("Dinner");
        result.Value.Suggestions.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNonExistentMealId_ShouldReturnNotFoundError()
    {
        // Arrange
        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var preferencesRepository = new InMemoryUserPreferencesRepository();
        var allergenDetector = new StubAllergenDetector();

        var handler = new GetSuggestionsQueryHandler(mealRepository, recipeRepository, preferencesRepository, allergenDetector);
        var query = new GetSuggestionsQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Meal.NotFound");
    }

    [Fact]
    public async Task Handle_WithDietaryPreference_ShouldFilterByTags()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var currentRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var preferencesRepository = new InMemoryUserPreferencesRepository();
        var allergenDetector = new StubAllergenDetector();

        var currentRecipe = RecipeBuilder.Create().WithId(currentRecipeId).WithName("Current").WithTags("Vegetarian").Build();
        var vegetarianRecipe = RecipeBuilder.Create().WithName("Veggie Pasta").WithTags("Vegetarian").Build();
        var nonVegetarianRecipe = RecipeBuilder.Create().WithName("Beef Steak").WithTags("Meat").Build();

        var meal = new PlannedMeal(mealId, date, MealType.Dinner, currentRecipeId);
        var preferences = new UserPreferences(DietaryPreference.Vegetarian, []);

        mealRepository.AddMeal(meal);
        recipeRepository.AddRecipe(currentRecipe);
        recipeRepository.AddRecipe(vegetarianRecipe);
        recipeRepository.AddRecipe(nonVegetarianRecipe);
        preferencesRepository.SetPreferences(preferences);

        var handler = new GetSuggestionsQueryHandler(mealRepository, recipeRepository, preferencesRepository, allergenDetector);
        var query = new GetSuggestionsQuery(mealId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Suggestions.Should().HaveCount(1);
        result.Value.Suggestions[0].Name.Should().Be("Veggie Pasta");
    }

    [Fact]
    public async Task Handle_WithAllergies_ShouldFilterOutAllergicRecipes()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var currentRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var preferencesRepository = new InMemoryUserPreferencesRepository();
        var allergenDetector = new StubAllergenDetector();

        var currentRecipe = RecipeBuilder.Create().WithId(currentRecipeId).WithName("Current").Build();
        var safeRecipe = RecipeBuilder.Create().WithName("Safe Recipe").Build();
        var allergicRecipe = RecipeBuilder.Create().WithName("Contains Allergen").Build();

        allergenDetector.AddAllergicRecipe(allergicRecipe.Id);

        var meal = new PlannedMeal(mealId, date, MealType.Dinner, currentRecipeId);
        var preferences = new UserPreferences(DietaryPreference.None, [Allergy.Dairy]);

        mealRepository.AddMeal(meal);
        recipeRepository.AddRecipe(currentRecipe);
        recipeRepository.AddRecipe(safeRecipe);
        recipeRepository.AddRecipe(allergicRecipe);
        preferencesRepository.SetPreferences(preferences);

        var handler = new GetSuggestionsQueryHandler(mealRepository, recipeRepository, preferencesRepository, allergenDetector);
        var query = new GetSuggestionsQuery(mealId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Suggestions.Should().HaveCount(1);
        result.Value.Suggestions[0].Name.Should().Be("Safe Recipe");
    }

    [Fact]
    public async Task Handle_ShouldExcludeCurrentRecipe()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var currentRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var preferencesRepository = new InMemoryUserPreferencesRepository();
        var allergenDetector = new StubAllergenDetector();

        var currentRecipe = RecipeBuilder.Create().WithId(currentRecipeId).WithName("Current Recipe").Build();
        var otherRecipe = RecipeBuilder.Create().WithName("Other Recipe").Build();

        var meal = new PlannedMeal(mealId, date, MealType.Dinner, currentRecipeId);

        mealRepository.AddMeal(meal);
        recipeRepository.AddRecipe(currentRecipe);
        recipeRepository.AddRecipe(otherRecipe);

        var handler = new GetSuggestionsQueryHandler(mealRepository, recipeRepository, preferencesRepository, allergenDetector);
        var query = new GetSuggestionsQuery(mealId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Suggestions.Should().HaveCount(1);
        result.Value.Suggestions.Should().NotContain(s => s.Name == "Current Recipe");
    }

    [Fact]
    public async Task Handle_WithNoMatchingPreferences_ShouldReturnEmptySuggestions()
    {
        // Arrange
        var mealId = Guid.NewGuid();
        var currentRecipeId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Now);

        var mealRepository = new InMemoryPlannedMealRepository();
        var recipeRepository = new InMemoryRecipeRepository();
        var preferencesRepository = new InMemoryUserPreferencesRepository();
        var allergenDetector = new StubAllergenDetector();

        var currentRecipe = RecipeBuilder.Create().WithId(currentRecipeId).WithName("Current").WithTags("Vegan").Build();
        var meatRecipe = RecipeBuilder.Create().WithName("Beef").WithTags("Meat").Build();

        var meal = new PlannedMeal(mealId, date, MealType.Dinner, currentRecipeId);
        var preferences = new UserPreferences(DietaryPreference.Vegan, []);

        mealRepository.AddMeal(meal);
        recipeRepository.AddRecipe(currentRecipe);
        recipeRepository.AddRecipe(meatRecipe);
        preferencesRepository.SetPreferences(preferences);

        var handler = new GetSuggestionsQueryHandler(mealRepository, recipeRepository, preferencesRepository, allergenDetector);
        var query = new GetSuggestionsQuery(mealId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Suggestions.Should().BeEmpty();
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

    private sealed class InMemoryUserPreferencesRepository : IUserPreferencesRepository
    {
        private UserPreferences _preferences = new(DietaryPreference.None, []);

        public void SetPreferences(UserPreferences preferences) => _preferences = preferences;

        public Task<UserPreferences> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_preferences);

        public Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
        {
            _preferences = preferences;
            return Task.CompletedTask;
        }
    }

    private sealed class StubAllergenDetector : IAllergenDetector
    {
        private readonly HashSet<Guid> _allergicRecipeIds = [];

        public void AddAllergicRecipe(Guid recipeId) => _allergicRecipeIds.Add(recipeId);

        public bool ContainsAllergen(Recipe recipe, IReadOnlyList<Allergy> allergies)
            => _allergicRecipeIds.Contains(recipe.Id);
    }
}
