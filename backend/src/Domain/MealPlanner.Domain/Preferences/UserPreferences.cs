namespace MealPlanner.Domain.Preferences;

public sealed class UserPreferences
{
    public DietaryPreference DietaryPreference { get; private set; }
    public IReadOnlyList<Allergy> Allergies { get; private set; }

    private UserPreferences()
    {
        DietaryPreference = DietaryPreference.None;
        Allergies = [];
    }

    public UserPreferences(
        DietaryPreference? dietaryPreference = null,
        IReadOnlyList<Allergy>? allergies = null)
    {
        DietaryPreference = dietaryPreference ?? DietaryPreference.None;
        Allergies = allergies ?? [];
    }

    public void Update(DietaryPreference dietaryPreference, IReadOnlyList<Allergy> allergies)
    {
        DietaryPreference = dietaryPreference;
        Allergies = allergies;
    }
}
