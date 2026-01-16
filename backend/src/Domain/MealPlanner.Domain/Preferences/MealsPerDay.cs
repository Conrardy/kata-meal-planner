namespace MealPlanner.Domain.Preferences;

public sealed record MealsPerDay
{
    public static readonly MealsPerDay Two = new(2);
    public static readonly MealsPerDay Three = new(3);
    public static readonly MealsPerDay Four = new(4);

    public int Value { get; }

    private MealsPerDay(int value) => Value = value;

    public static MealsPerDay FromInt(int value) =>
        value switch
        {
            2 => Two,
            3 => Three,
            4 => Four,
            _ => throw new ArgumentException($"Invalid meals per day: {value}. Must be 2, 3, or 4.", nameof(value))
        };

    public static IReadOnlyList<MealsPerDay> All => [Two, Three, Four];

    public override string ToString() => Value.ToString();
}
