namespace MealPlanner.Infrastructure.Persistence;

public sealed class UserPreferencesEntity
{
    public Guid Id { get; set; }
    public string DietaryPreference { get; set; } = "None";
    public List<string> Allergies { get; set; } = [];
    public int MealsPerDay { get; set; } = 3;
    public int PlanLength { get; set; } = 1;
    public bool IncludeLeftovers { get; set; }
    public bool AutoGenerateShoppingList { get; set; } = true;
    public List<string> ExcludedIngredients { get; set; } = [];
}
