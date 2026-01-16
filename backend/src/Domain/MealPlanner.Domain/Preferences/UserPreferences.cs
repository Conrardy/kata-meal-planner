namespace MealPlanner.Domain.Preferences;

public sealed class UserPreferences
{
    public DietaryPreference DietaryPreference { get; private set; }
    public IReadOnlyList<Allergy> Allergies { get; private set; }
    public MealsPerDay MealsPerDay { get; private set; }
    public PlanLength PlanLength { get; private set; }
    public bool IncludeLeftovers { get; private set; }
    public bool AutoGenerateShoppingList { get; private set; }
    public IReadOnlyList<string> ExcludedIngredients { get; private set; }

    private UserPreferences()
    {
        DietaryPreference = DietaryPreference.None;
        Allergies = [];
        MealsPerDay = MealsPerDay.Three;
        PlanLength = PlanLength.OneWeek;
        IncludeLeftovers = false;
        AutoGenerateShoppingList = true;
        ExcludedIngredients = [];
    }

    public UserPreferences(
        DietaryPreference? dietaryPreference = null,
        IReadOnlyList<Allergy>? allergies = null,
        MealsPerDay? mealsPerDay = null,
        PlanLength? planLength = null,
        bool? includeLeftovers = null,
        bool? autoGenerateShoppingList = null,
        IReadOnlyList<string>? excludedIngredients = null)
    {
        DietaryPreference = dietaryPreference ?? DietaryPreference.None;
        Allergies = allergies ?? [];
        MealsPerDay = mealsPerDay ?? MealsPerDay.Three;
        PlanLength = planLength ?? PlanLength.OneWeek;
        IncludeLeftovers = includeLeftovers ?? false;
        AutoGenerateShoppingList = autoGenerateShoppingList ?? true;
        ExcludedIngredients = excludedIngredients ?? [];
    }

    public void Update(DietaryPreference dietaryPreference, IReadOnlyList<Allergy> allergies)
    {
        DietaryPreference = dietaryPreference;
        Allergies = allergies;
    }

    public void UpdateMealPlanOptions(
        MealsPerDay mealsPerDay,
        PlanLength planLength,
        bool includeLeftovers,
        bool autoGenerateShoppingList,
        IReadOnlyList<string> excludedIngredients)
    {
        MealsPerDay = mealsPerDay;
        PlanLength = planLength;
        IncludeLeftovers = includeLeftovers;
        AutoGenerateShoppingList = autoGenerateShoppingList;
        ExcludedIngredients = excludedIngredients;
    }
}
