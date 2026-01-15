namespace MealPlanner.Application.Meals;

public sealed record SuggestionsDto(
    Guid MealId,
    string MealType,
    IReadOnlyList<RecipeSuggestionDto> Suggestions
);

public sealed record RecipeSuggestionDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    string? Description
);
