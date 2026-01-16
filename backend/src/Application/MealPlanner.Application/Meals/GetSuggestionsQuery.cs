using MediatR;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Preferences;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Meals;

public sealed record GetSuggestionsQuery(Guid MealId) : IRequest<SuggestionsDto>;

public sealed class GetSuggestionsQueryHandler : IRequestHandler<GetSuggestionsQuery, SuggestionsDto>
{
    private readonly IPlannedMealRepository _mealRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUserPreferencesRepository _preferencesRepository;

    public GetSuggestionsQueryHandler(
        IPlannedMealRepository mealRepository,
        IRecipeRepository recipeRepository,
        IUserPreferencesRepository preferencesRepository)
    {
        _mealRepository = mealRepository;
        _recipeRepository = recipeRepository;
        _preferencesRepository = preferencesRepository;
    }

    public async Task<SuggestionsDto> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var meal = await _mealRepository.GetByIdAsync(request.MealId, cancellationToken);
        if (meal == null)
        {
            throw new InvalidOperationException($"Meal with ID {request.MealId} not found");
        }

        var suggestions = await _recipeRepository.GetSuggestionsAsync(meal.RecipeId, cancellationToken);
        var preferences = await _preferencesRepository.GetAsync(cancellationToken);

        var filteredSuggestions = FilterByPreferences(suggestions, preferences);

        var suggestionDtos = filteredSuggestions
            .Select(r => new RecipeSuggestionDto(r.Id, r.Name, r.ImageUrl, r.Description))
            .ToList();

        return new SuggestionsDto(request.MealId, meal.MealType.Value, suggestionDtos);
    }

    private static IEnumerable<Recipe> FilterByPreferences(IReadOnlyList<Recipe> recipes, UserPreferences preferences)
    {
        var filtered = recipes.AsEnumerable();

        if (preferences.DietaryPreference != DietaryPreference.None)
        {
            var dietaryTag = preferences.DietaryPreference.Value;
            filtered = filtered.Where(r => r.Tags.Contains(dietaryTag, StringComparer.OrdinalIgnoreCase));
        }

        if (preferences.Allergies.Count > 0)
        {
            filtered = filtered.Where(r => !ContainsAllergen(r, preferences.Allergies));
        }

        return filtered;
    }

    private static bool ContainsAllergen(Recipe recipe, IReadOnlyList<Allergy> allergies)
    {
        foreach (var allergy in allergies)
        {
            if (allergy == Allergy.Gluten && !recipe.Tags.Contains("Gluten-Free", StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            if (allergy == Allergy.Dairy && recipe.Ingredients.Any(i => ContainsDairy(i.Name)))
            {
                return true;
            }
            if (allergy == Allergy.Nuts && recipe.Ingredients.Any(i => ContainsNuts(i.Name)))
            {
                return true;
            }
            if (allergy == Allergy.Eggs && recipe.Ingredients.Any(i => ContainsEggs(i.Name)))
            {
                return true;
            }
            if (allergy == Allergy.Shellfish && recipe.Ingredients.Any(i => ContainsShellfish(i.Name)))
            {
                return true;
            }
            if (allergy == Allergy.Soy && recipe.Ingredients.Any(i => ContainsSoy(i.Name)))
            {
                return true;
            }
        }
        return false;
    }

    private static bool ContainsDairy(string ingredientName)
    {
        var dairyKeywords = new[] { "milk", "cheese", "cream", "butter", "yogurt", "parmesan", "dairy" };
        return dairyKeywords.Any(k => ingredientName.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsNuts(string ingredientName)
    {
        var nutKeywords = new[] { "nut", "almond", "walnut", "pecan", "cashew", "pistachio", "hazelnut", "peanut" };
        return nutKeywords.Any(k => ingredientName.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsEggs(string ingredientName)
    {
        var eggKeywords = new[] { "egg", "eggs" };
        return eggKeywords.Any(k => ingredientName.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsShellfish(string ingredientName)
    {
        var shellfishKeywords = new[] { "shrimp", "lobster", "crab", "oyster", "mussel", "clam", "scallop", "shellfish" };
        return shellfishKeywords.Any(k => ingredientName.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsSoy(string ingredientName)
    {
        var soyKeywords = new[] { "soy", "tofu", "edamame", "tempeh", "miso" };
        return soyKeywords.Any(k => ingredientName.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
