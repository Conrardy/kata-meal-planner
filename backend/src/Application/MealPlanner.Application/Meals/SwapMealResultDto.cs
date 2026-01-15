namespace MealPlanner.Application.Meals;

public sealed record SwapMealResultDto(
    Guid MealId,
    string MealType,
    string RecipeName,
    string? ImageUrl
);
