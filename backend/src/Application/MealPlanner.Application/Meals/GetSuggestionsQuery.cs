using MediatR;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Meals;

public sealed record GetSuggestionsQuery(Guid MealId) : IRequest<SuggestionsDto>;

public sealed class GetSuggestionsQueryHandler : IRequestHandler<GetSuggestionsQuery, SuggestionsDto>
{
    private readonly IPlannedMealRepository _mealRepository;
    private readonly IRecipeRepository _recipeRepository;

    public GetSuggestionsQueryHandler(IPlannedMealRepository mealRepository, IRecipeRepository recipeRepository)
    {
        _mealRepository = mealRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<SuggestionsDto> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var meal = await _mealRepository.GetByIdAsync(request.MealId, cancellationToken);
        if (meal == null)
        {
            throw new InvalidOperationException($"Meal with ID {request.MealId} not found");
        }

        var suggestions = await _recipeRepository.GetSuggestionsAsync(meal.RecipeId, cancellationToken);

        var suggestionDtos = suggestions
            .Select(r => new RecipeSuggestionDto(r.Id, r.Name, r.ImageUrl, r.Description))
            .ToList();

        return new SuggestionsDto(request.MealId, meal.MealType.Value, suggestionDtos);
    }
}
