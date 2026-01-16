using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class InMemoryPlannedMealRepository : IPlannedMealRepository, IRecipeRepository
{
    private readonly List<PlannedMeal> _meals;
    private readonly List<Recipe> _recipes;

    public InMemoryPlannedMealRepository()
    {
        _recipes = GenerateSampleRecipes();
        _meals = GenerateSampleMeals(_recipes);
    }

    public Task<IReadOnlyList<PlannedMeal>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var meals = _meals
            .Where(m => m.Date == date)
            .ToList();

        foreach (var meal in meals)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == meal.RecipeId);
            if (recipe != null)
            {
                SetRecipe(meal, recipe);
            }
        }

        return Task.FromResult<IReadOnlyList<PlannedMeal>>(meals);
    }

    public Task<IReadOnlyList<PlannedMeal>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var meals = _meals
            .Where(m => m.Date >= startDate && m.Date <= endDate)
            .ToList();

        foreach (var meal in meals)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == meal.RecipeId);
            if (recipe != null)
            {
                SetRecipe(meal, recipe);
            }
        }

        return Task.FromResult<IReadOnlyList<PlannedMeal>>(meals);
    }

    public Task<PlannedMeal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var meal = _meals.FirstOrDefault(m => m.Id == id);
        if (meal != null)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == meal.RecipeId);
            if (recipe != null)
            {
                SetRecipe(meal, recipe);
            }
        }
        return Task.FromResult(meal);
    }

    public Task UpdateAsync(PlannedMeal meal, CancellationToken cancellationToken = default)
    {
        var existingMeal = _meals.FirstOrDefault(m => m.Id == meal.Id);
        if (existingMeal != null)
        {
            var recipe = _recipes.FirstOrDefault(r => r.Id == meal.RecipeId);
            if (recipe != null)
            {
                SetRecipe(meal, recipe);
            }
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Recipe>>(_recipes.ToList());
    }

    Task<Recipe?> IRecipeRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var recipe = _recipes.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(recipe);
    }

    public Task<IReadOnlyList<Recipe>> GetSuggestionsAsync(Guid excludeRecipeId, CancellationToken cancellationToken = default)
    {
        var suggestions = _recipes
            .Where(r => r.Id != excludeRecipeId)
            .ToList();
        return Task.FromResult<IReadOnlyList<Recipe>>(suggestions);
    }

    public Task<IReadOnlyList<Recipe>> SearchAsync(string? searchTerm, IReadOnlyList<string>? tags, CancellationToken cancellationToken = default)
    {
        var query = _recipes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(r =>
                r.Name.ToLowerInvariant().Contains(term) ||
                (r.Description?.ToLowerInvariant().Contains(term) ?? false));
        }

        if (tags is { Count: > 0 })
        {
            query = query.Where(r => tags.Any(t => r.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)));
        }

        return Task.FromResult<IReadOnlyList<Recipe>>(query.ToList());
    }

    public Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        var allTags = _recipes
            .SelectMany(r => r.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
        return Task.FromResult<IReadOnlyList<string>>(allTags);
    }

    private static void SetRecipe(PlannedMeal meal, Recipe recipe)
    {
        var recipeProperty = typeof(PlannedMeal).GetProperty(nameof(PlannedMeal.Recipe));
        recipeProperty?.SetValue(meal, recipe);
    }

    private static List<Recipe> GenerateSampleRecipes() =>
    [
        new Recipe(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Oatmeal with Berries",
            "https://images.unsplash.com/photo-1517673400267-0251440c45dc?w=400",
            "Healthy breakfast oatmeal topped with fresh berries",
            ["Quick & Easy", "Vegetarian", "Breakfast"]
        ),
        new Recipe(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Grilled Chicken Salad",
            "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400",
            "Fresh salad with grilled chicken breast",
            ["Low Carb", "Gluten-Free", "Lunch"]
        ),
        new Recipe(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Spaghetti Bolognese",
            "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400",
            "Classic Italian pasta with meat sauce",
            ["Family-Friendly", "Dinner"]
        ),
        new Recipe(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Avocado Toast",
            "https://images.unsplash.com/photo-1541519227354-08fa5d50c44d?w=400",
            "Crispy toast with fresh avocado and eggs",
            ["Quick & Easy", "Vegetarian", "Breakfast"]
        ),
        new Recipe(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Caesar Salad",
            "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400",
            "Classic Caesar salad with parmesan and croutons",
            ["Vegetarian", "Quick & Easy", "Lunch"]
        ),
        new Recipe(
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Grilled Salmon",
            "https://images.unsplash.com/photo-1467003909585-2f8a72700288?w=400",
            "Fresh grilled salmon with vegetables",
            ["Gluten-Free", "Low Carb", "Dinner"]
        ),
        new Recipe(
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            "Vegetable Stir Fry",
            "https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400",
            "Quick and healthy vegetable stir fry with tofu",
            ["Vegetarian", "Quick & Easy", "Low Carb", "Dinner"]
        ),
        new Recipe(
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            "Chocolate Brownie",
            "https://images.unsplash.com/photo-1564355808539-22fda35bed7e?w=400",
            "Rich and fudgy chocolate brownies",
            ["Vegetarian", "Desserts"]
        ),
        new Recipe(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "Greek Yogurt Parfait",
            "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=400",
            "Layered yogurt with granola and fresh fruits",
            ["Quick & Easy", "Vegetarian", "Gluten-Free", "Breakfast"]
        ),
        new Recipe(
            Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
            "Beef Tacos",
            "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400",
            "Seasoned beef tacos with fresh toppings",
            ["Family-Friendly", "Quick & Easy", "Dinner"]
        )
    ];

    private static List<PlannedMeal> GenerateSampleMeals(List<Recipe> recipes)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var meals = new List<PlannedMeal>();
        var mealTypes = new[] { MealType.Breakfast, MealType.Lunch, MealType.Dinner };

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var date = startOfWeek.AddDays(dayOffset);
            foreach (var mealType in mealTypes)
            {
                var recipeIndex = (dayOffset + Array.IndexOf(mealTypes, mealType)) % recipes.Count;
                var mealId = Guid.NewGuid();

                if (date == today)
                {
                    mealId = mealType == MealType.Breakfast
                        ? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                        : mealType == MealType.Lunch
                            ? Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                            : Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
                }

                meals.Add(new PlannedMeal(mealId, date, mealType, recipes[recipeIndex].Id));
            }
        }

        return meals;
    }
}
