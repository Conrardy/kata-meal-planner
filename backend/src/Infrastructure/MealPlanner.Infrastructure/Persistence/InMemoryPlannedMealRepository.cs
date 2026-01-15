using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class InMemoryPlannedMealRepository : IPlannedMealRepository
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
