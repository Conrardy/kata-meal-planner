using ErrorOr;

namespace MealPlanner.Application.Meals;

public static class MealErrors
{
    public static Error NotFound(Guid mealId) =>
        Error.NotFound("Meal.NotFound", $"Meal with ID {mealId} was not found.");

    public static Error RecipeNotFound(Guid recipeId) =>
        Error.NotFound("Meal.RecipeNotFound", $"Recipe with ID {recipeId} was not found.");

    public static Error SlotAlreadyFilled(DateOnly date, string mealType) =>
        Error.Conflict("Meal.SlotAlreadyFilled", $"A meal is already planned for {mealType} on {date:yyyy-MM-dd}. Use swap instead.");
}
