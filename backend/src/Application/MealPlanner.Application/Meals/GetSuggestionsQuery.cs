using ErrorOr;
using MediatR;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Preferences;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Meals;

public sealed record GetSuggestionsQuery(Guid MealId) : IRequest<ErrorOr<SuggestionsDto>>;

public sealed class GetSuggestionsQueryHandler : IRequestHandler<GetSuggestionsQuery, ErrorOr<SuggestionsDto>>
{
    private readonly IPlannedMealRepository _mealRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUserPreferencesRepository _preferencesRepository;
    private readonly IAllergenDetector _allergenDetector;

    public GetSuggestionsQueryHandler(
        IPlannedMealRepository mealRepository,
        IRecipeRepository recipeRepository,
        IUserPreferencesRepository preferencesRepository,
        IAllergenDetector allergenDetector)
    {
        _mealRepository = mealRepository;
        _recipeRepository = recipeRepository;
        _preferencesRepository = preferencesRepository;
        _allergenDetector = allergenDetector;
    }

    public async Task<ErrorOr<SuggestionsDto>> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var meal = await _mealRepository.GetByIdAsync(request.MealId, cancellationToken);
        if (meal == null)
        {
            return MealErrors.NotFound(request.MealId);
        }

        var suggestions = await _recipeRepository.GetSuggestionsAsync(meal.RecipeId, cancellationToken);
        var preferences = await _preferencesRepository.GetAsync(cancellationToken);

        var filteredSuggestions = FilterByPreferences(suggestions, preferences);

        var suggestionDtos = filteredSuggestions
            .Select(r => new RecipeSuggestionDto(r.Id, r.Name, r.ImageUrl, r.Description))
            .ToList();

        return new SuggestionsDto(request.MealId, meal.MealType.Value, suggestionDtos);
    }

    private IEnumerable<Recipe> FilterByPreferences(IReadOnlyList<Recipe> recipes, UserPreferences preferences)
    {
        var filtered = recipes.AsEnumerable();

        if (preferences.DietaryPreference != DietaryPreference.None)
        {
            var dietaryTag = preferences.DietaryPreference.Value;
            filtered = filtered.Where(r => r.Tags.Contains(dietaryTag, StringComparer.OrdinalIgnoreCase));
        }

        if (preferences.Allergies.Count > 0)
        {
            filtered = filtered.Where(r => !_allergenDetector.ContainsAllergen(r, preferences.Allergies));
        }

        return filtered;
    }
}
