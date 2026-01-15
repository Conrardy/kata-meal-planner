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
            "Healthy breakfast oatmeal topped with fresh berries"
        ),
        new Recipe(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Grilled Chicken Salad",
            "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400",
            "Fresh salad with grilled chicken breast"
        ),
        new Recipe(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Spaghetti Bolognese",
            "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400",
            "Classic Italian pasta with meat sauce"
        ),
        new Recipe(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Avocado Toast",
            "https://images.unsplash.com/photo-1541519227354-08fa5d50c44d?w=400",
            "Crispy toast with fresh avocado and eggs"
        ),
        new Recipe(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Caesar Salad",
            "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400",
            "Classic Caesar salad with parmesan and croutons"
        ),
        new Recipe(
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Grilled Salmon",
            "https://images.unsplash.com/photo-1467003909585-2f8a72700288?w=400",
            "Fresh grilled salmon with vegetables"
        )
    ];

    private static List<PlannedMeal> GenerateSampleMeals(List<Recipe> recipes)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return
        [
            new PlannedMeal(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                today,
                MealType.Breakfast,
                recipes[0].Id
            ),
            new PlannedMeal(
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                today,
                MealType.Lunch,
                recipes[1].Id
            ),
            new PlannedMeal(
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                today,
                MealType.Dinner,
                recipes[2].Id
            )
        ];
    }
}
