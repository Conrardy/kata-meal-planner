namespace MealPlanner.Domain.Preferences;

public sealed record DietaryPreference
{
    public static readonly DietaryPreference None = new("None");
    public static readonly DietaryPreference Vegetarian = new("Vegetarian");
    public static readonly DietaryPreference Vegan = new("Vegan");
    public static readonly DietaryPreference Pescatarian = new("Pescatarian");
    public static readonly DietaryPreference Keto = new("Keto");
    public static readonly DietaryPreference Paleo = new("Paleo");
    public static readonly DietaryPreference LowCarb = new("Low Carb");
    public static readonly DietaryPreference Mediterranean = new("Mediterranean");

    public string Value { get; }

    private DietaryPreference(string value) => Value = value;

    public static DietaryPreference FromString(string value) =>
        value.ToLowerInvariant() switch
        {
            "none" => None,
            "vegetarian" => Vegetarian,
            "vegan" => Vegan,
            "pescatarian" => Pescatarian,
            "keto" => Keto,
            "paleo" => Paleo,
            "low carb" or "lowcarb" => LowCarb,
            "mediterranean" => Mediterranean,
            _ => throw new ArgumentException($"Invalid dietary preference: {value}", nameof(value))
        };

    public static IReadOnlyList<DietaryPreference> All =>
    [
        None,
        Vegetarian,
        Vegan,
        Pescatarian,
        Keto,
        Paleo,
        LowCarb,
        Mediterranean
    ];

    public override string ToString() => Value;
}
