using MealPlanner.Domain.Recipes;

namespace MealPlanner.Domain.Meals;

public sealed class PlannedMeal
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public MealType MealType { get; private set; }
    public Guid RecipeId { get; private set; }
    public Recipe? Recipe { get; private set; }

    private PlannedMeal()
    {
        MealType = MealType.Breakfast;
    }

    public PlannedMeal(Guid id, DateOnly date, MealType mealType, Guid recipeId)
    {
        Id = id;
        Date = date;
        MealType = mealType;
        RecipeId = recipeId;
    }

    public void SwapRecipe(Guid newRecipeId)
    {
        RecipeId = newRecipeId;
        Recipe = null;
    }
}
