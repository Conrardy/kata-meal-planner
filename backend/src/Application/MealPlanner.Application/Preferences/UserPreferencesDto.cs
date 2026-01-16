namespace MealPlanner.Application.Preferences;

public sealed record UserPreferencesDto(
    string DietaryPreference,
    IReadOnlyList<string> Allergies,
    IReadOnlyList<string> AvailableDietaryPreferences,
    IReadOnlyList<string> AvailableAllergies
);
