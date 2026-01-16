namespace MealPlanner.Application.Preferences;

public sealed record UserPreferencesDto(
    string DietaryPreference,
    IReadOnlyList<string> Allergies,
    IReadOnlyList<string> AvailableDietaryPreferences,
    IReadOnlyList<string> AvailableAllergies,
    int MealsPerDay,
    int PlanLength,
    bool IncludeLeftovers,
    bool AutoGenerateShoppingList,
    IReadOnlyList<string> ExcludedIngredients,
    IReadOnlyList<int> AvailableMealsPerDay,
    IReadOnlyList<int> AvailablePlanLengths
);
