namespace MealPlanner.Domain.Preferences;

public sealed record Allergy
{
    public static readonly Allergy Gluten = new("Gluten");
    public static readonly Allergy Nuts = new("Nuts");
    public static readonly Allergy Dairy = new("Dairy");
    public static readonly Allergy Eggs = new("Eggs");
    public static readonly Allergy Shellfish = new("Shellfish");
    public static readonly Allergy Soy = new("Soy");

    public string Value { get; }

    private Allergy(string value) => Value = value;

    public static Allergy FromString(string value) =>
        value.ToLowerInvariant() switch
        {
            "gluten" => Gluten,
            "nuts" => Nuts,
            "dairy" => Dairy,
            "eggs" => Eggs,
            "shellfish" => Shellfish,
            "soy" => Soy,
            _ => throw new ArgumentException($"Invalid allergy: {value}", nameof(value))
        };

    public static IReadOnlyList<Allergy> All =>
    [
        Gluten,
        Nuts,
        Dairy,
        Eggs,
        Shellfish,
        Soy
    ];

    public override string ToString() => Value;
}
