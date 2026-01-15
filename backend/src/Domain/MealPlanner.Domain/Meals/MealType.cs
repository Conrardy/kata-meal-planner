namespace MealPlanner.Domain.Meals;

public sealed record MealType
{
    public static readonly MealType Breakfast = new("Breakfast");
    public static readonly MealType Lunch = new("Lunch");
    public static readonly MealType Dinner = new("Dinner");

    public string Value { get; }

    private MealType(string value) => Value = value;

    public static MealType FromString(string value) =>
        value.ToLowerInvariant() switch
        {
            "breakfast" => Breakfast,
            "lunch" => Lunch,
            "dinner" => Dinner,
            _ => throw new ArgumentException($"Invalid meal type: {value}", nameof(value))
        };

    public override string ToString() => Value;
}
