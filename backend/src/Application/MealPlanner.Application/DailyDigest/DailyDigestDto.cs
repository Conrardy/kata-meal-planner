namespace MealPlanner.Application.DailyDigest;

public sealed record DailyDigestDto(
    DateOnly Date,
    IReadOnlyList<PlannedMealDto> Meals
);

public sealed record PlannedMealDto(
    Guid Id,
    string MealType,
    Guid RecipeId,
    string RecipeName,
    string? ImageUrl
);
