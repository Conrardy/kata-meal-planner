namespace MealPlanner.Application.WeeklyPlan;

public sealed record WeeklyPlanDto(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<DayPlanDto> Days
);

public sealed record DayPlanDto(
    DateOnly Date,
    string DayName,
    WeeklyMealDto? Breakfast,
    WeeklyMealDto? Lunch,
    WeeklyMealDto? Dinner
);

public sealed record WeeklyMealDto(
    Guid Id,
    Guid RecipeId,
    string RecipeName,
    string? ImageUrl
);
