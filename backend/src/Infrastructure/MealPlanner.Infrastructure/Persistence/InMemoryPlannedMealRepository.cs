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

    public Task<PlannedMeal?> GetByDateAndMealTypeAsync(DateOnly date, MealType mealType, CancellationToken cancellationToken = default)
    {
        var meal = _meals.FirstOrDefault(m => m.Date == date && m.MealType == mealType);
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

    public Task AddAsync(PlannedMeal meal, CancellationToken cancellationToken = default)
    {
        var existing = _meals.FirstOrDefault(m => m.Date == meal.Date && m.MealType == meal.MealType);
        if (existing != null)
        {
            _meals.Remove(existing);
        }
        _meals.Add(meal);

        var recipe = _recipes.FirstOrDefault(r => r.Id == meal.RecipeId);
        if (recipe != null)
        {
            SetRecipe(meal, recipe);
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
            ["Quick & Easy", "Vegetarian", "Breakfast"],
            MealType.Breakfast,
            [
                new Ingredient("Rolled oats", "1", "cup"),
                new Ingredient("Milk", "2", "cups"),
                new Ingredient("Mixed berries", "1/2", "cup"),
                new Ingredient("Honey", "1", "tbsp"),
                new Ingredient("Cinnamon", "1/4", "tsp")
            ],
            [
                new CookingStep(1, "Bring milk to a boil in a medium saucepan."),
                new CookingStep(2, "Add oats and reduce heat to medium-low."),
                new CookingStep(3, "Cook for 5 minutes, stirring occasionally."),
                new CookingStep(4, "Top with berries, honey, and cinnamon.")
            ]
        ),
        new Recipe(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Grilled Chicken Salad",
            "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400",
            "Fresh salad with grilled chicken breast",
            ["Low Carb", "Gluten-Free", "Lunch"],
            MealType.Lunch,
            [
                new Ingredient("Chicken breast", "200", "g"),
                new Ingredient("Mixed greens", "4", "cups"),
                new Ingredient("Cherry tomatoes", "1", "cup"),
                new Ingredient("Cucumber", "1", null),
                new Ingredient("Olive oil", "2", "tbsp"),
                new Ingredient("Lemon juice", "1", "tbsp"),
                new Ingredient("Salt and pepper", "to taste", null)
            ],
            [
                new CookingStep(1, "Season chicken breast with salt and pepper."),
                new CookingStep(2, "Grill chicken for 6-7 minutes per side until cooked through."),
                new CookingStep(3, "Let chicken rest for 5 minutes, then slice."),
                new CookingStep(4, "Combine greens, tomatoes, and cucumber in a bowl."),
                new CookingStep(5, "Top with sliced chicken and drizzle with olive oil and lemon juice.")
            ]
        ),
        new Recipe(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Spaghetti Bolognese",
            "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400",
            "Classic Italian pasta with meat sauce",
            ["Family-Friendly", "Dinner"],
            MealType.Dinner,
            [
                new Ingredient("Spaghetti", "400", "g"),
                new Ingredient("Ground beef", "500", "g"),
                new Ingredient("Onion", "1", null),
                new Ingredient("Garlic cloves", "3", null),
                new Ingredient("Canned tomatoes", "400", "g"),
                new Ingredient("Tomato paste", "2", "tbsp"),
                new Ingredient("Italian herbs", "1", "tsp"),
                new Ingredient("Parmesan cheese", "50", "g")
            ],
            [
                new CookingStep(1, "Cook spaghetti according to package instructions."),
                new CookingStep(2, "Sauté diced onion and garlic in olive oil until soft."),
                new CookingStep(3, "Add ground beef and cook until browned."),
                new CookingStep(4, "Stir in tomatoes, tomato paste, and herbs. Simmer for 20 minutes."),
                new CookingStep(5, "Serve sauce over spaghetti and top with parmesan.")
            ]
        ),
        new Recipe(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Avocado Toast",
            "https://images.unsplash.com/photo-1541519227354-08fa5d50c44d?w=400",
            "Crispy toast with fresh avocado and eggs",
            ["Quick & Easy", "Vegetarian", "Breakfast"],
            MealType.Breakfast,
            [
                new Ingredient("Bread slices", "2", null),
                new Ingredient("Ripe avocado", "1", null),
                new Ingredient("Eggs", "2", null),
                new Ingredient("Red pepper flakes", "1/4", "tsp"),
                new Ingredient("Salt", "to taste", null),
                new Ingredient("Lemon juice", "1", "tsp")
            ],
            [
                new CookingStep(1, "Toast bread until golden and crispy."),
                new CookingStep(2, "Mash avocado with lemon juice and salt."),
                new CookingStep(3, "Poach or fry eggs to your preference."),
                new CookingStep(4, "Spread avocado on toast, top with eggs and red pepper flakes.")
            ]
        ),
        new Recipe(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Caesar Salad",
            "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400",
            "Classic Caesar salad with parmesan and croutons",
            ["Vegetarian", "Quick & Easy", "Lunch"],
            MealType.Lunch,
            [
                new Ingredient("Romaine lettuce", "1", "head"),
                new Ingredient("Parmesan cheese", "50", "g"),
                new Ingredient("Croutons", "1", "cup"),
                new Ingredient("Caesar dressing", "4", "tbsp"),
                new Ingredient("Lemon wedges", "2", null)
            ],
            [
                new CookingStep(1, "Wash and chop romaine lettuce into bite-sized pieces."),
                new CookingStep(2, "Shave parmesan cheese."),
                new CookingStep(3, "Toss lettuce with Caesar dressing."),
                new CookingStep(4, "Top with croutons, parmesan, and serve with lemon wedges.")
            ]
        ),
        new Recipe(
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Grilled Salmon",
            "https://images.unsplash.com/photo-1467003909585-2f8a72700288?w=400",
            "Fresh grilled salmon with vegetables",
            ["Gluten-Free", "Low Carb", "Dinner"],
            MealType.Dinner,
            [
                new Ingredient("Salmon fillet", "400", "g"),
                new Ingredient("Asparagus", "200", "g"),
                new Ingredient("Olive oil", "2", "tbsp"),
                new Ingredient("Lemon", "1", null),
                new Ingredient("Garlic powder", "1/2", "tsp"),
                new Ingredient("Salt and pepper", "to taste", null),
                new Ingredient("Fresh dill", "2", "tbsp")
            ],
            [
                new CookingStep(1, "Preheat grill to medium-high heat."),
                new CookingStep(2, "Season salmon with olive oil, garlic powder, salt, and pepper."),
                new CookingStep(3, "Grill salmon for 4-5 minutes per side."),
                new CookingStep(4, "Grill asparagus alongside for 3-4 minutes."),
                new CookingStep(5, "Serve with lemon wedges and fresh dill.")
            ]
        ),
        new Recipe(
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            "Vegetable Stir Fry",
            "https://images.unsplash.com/photo-1512058564366-18510be2db19?w=400",
            "Quick and healthy vegetable stir fry with tofu",
            ["Vegetarian", "Quick & Easy", "Low Carb", "Dinner"],
            MealType.Dinner,
            [
                new Ingredient("Firm tofu", "300", "g"),
                new Ingredient("Broccoli florets", "2", "cups"),
                new Ingredient("Bell peppers", "2", null),
                new Ingredient("Soy sauce", "3", "tbsp"),
                new Ingredient("Sesame oil", "1", "tbsp"),
                new Ingredient("Ginger", "1", "tbsp"),
                new Ingredient("Garlic cloves", "2", null)
            ],
            [
                new CookingStep(1, "Press and cube tofu, then pan-fry until golden."),
                new CookingStep(2, "Stir-fry ginger and garlic in sesame oil."),
                new CookingStep(3, "Add broccoli and peppers, cook for 3-4 minutes."),
                new CookingStep(4, "Add tofu and soy sauce, toss to combine."),
                new CookingStep(5, "Serve hot over rice if desired.")
            ]
        ),
        new Recipe(
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            "Chocolate Brownie",
            "https://images.unsplash.com/photo-1564355808539-22fda35bed7e?w=400",
            "Rich and fudgy chocolate brownies",
            ["Vegetarian", "Desserts"],
            MealType.Dinner,
            [
                new Ingredient("Dark chocolate", "200", "g"),
                new Ingredient("Butter", "150", "g"),
                new Ingredient("Sugar", "200", "g"),
                new Ingredient("Eggs", "3", null),
                new Ingredient("Flour", "100", "g"),
                new Ingredient("Vanilla extract", "1", "tsp"),
                new Ingredient("Salt", "1/4", "tsp")
            ],
            [
                new CookingStep(1, "Preheat oven to 350°F (175°C). Line a baking pan with parchment paper."),
                new CookingStep(2, "Melt chocolate and butter together, let cool slightly."),
                new CookingStep(3, "Whisk sugar and eggs until fluffy, add vanilla."),
                new CookingStep(4, "Fold in chocolate mixture, then flour and salt."),
                new CookingStep(5, "Pour into pan and bake for 25-30 minutes. Let cool before cutting.")
            ]
        ),
        new Recipe(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "Greek Yogurt Parfait",
            "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=400",
            "Layered yogurt with granola and fresh fruits",
            ["Quick & Easy", "Vegetarian", "Gluten-Free", "Breakfast"],
            MealType.Breakfast,
            [
                new Ingredient("Greek yogurt", "2", "cups"),
                new Ingredient("Granola", "1", "cup"),
                new Ingredient("Mixed berries", "1", "cup"),
                new Ingredient("Honey", "2", "tbsp"),
                new Ingredient("Chia seeds", "1", "tbsp")
            ],
            [
                new CookingStep(1, "Spoon a layer of yogurt into glasses or bowls."),
                new CookingStep(2, "Add a layer of granola."),
                new CookingStep(3, "Add a layer of berries."),
                new CookingStep(4, "Repeat layers and top with honey and chia seeds.")
            ]
        ),
        new Recipe(
            Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
            "Beef Tacos",
            "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=400",
            "Seasoned beef tacos with fresh toppings",
            ["Family-Friendly", "Quick & Easy", "Dinner"],
            MealType.Dinner,
            [
                new Ingredient("Ground beef", "500", "g"),
                new Ingredient("Taco seasoning", "2", "tbsp"),
                new Ingredient("Taco shells", "8", null),
                new Ingredient("Shredded lettuce", "2", "cups"),
                new Ingredient("Diced tomatoes", "1", "cup"),
                new Ingredient("Shredded cheese", "1", "cup"),
                new Ingredient("Sour cream", "1/2", "cup"),
                new Ingredient("Salsa", "1/2", "cup")
            ],
            [
                new CookingStep(1, "Brown ground beef in a skillet, drain excess fat."),
                new CookingStep(2, "Add taco seasoning and water as directed on packet."),
                new CookingStep(3, "Simmer for 5 minutes until sauce thickens."),
                new CookingStep(4, "Warm taco shells according to package instructions."),
                new CookingStep(5, "Assemble tacos with beef, lettuce, tomatoes, cheese, sour cream, and salsa.")
            ]
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
