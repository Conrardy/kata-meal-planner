namespace MealPlanner.Application.DailyDigest;

public sealed record DailyDigestDto(
    DateOnly Date,
    IReadOnlyList<PlannedMealDto> Meals
);

public sealed record PlannedMealDto(
    Guid Id,
    string MealType,
    string RecipeName,
    string? ImageUrl
);
