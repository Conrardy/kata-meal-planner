namespace MealPlanner.Domain.Preferences;

public sealed record PlanLength
{
    public static readonly PlanLength OneWeek = new(1);
    public static readonly PlanLength TwoWeeks = new(2);

    public int Value { get; }

    private PlanLength(int value) => Value = value;

    public static PlanLength FromInt(int value) =>
        value switch
        {
            1 => OneWeek,
            2 => TwoWeeks,
            _ => throw new ArgumentException($"Invalid plan length: {value}. Must be 1 or 2 weeks.", nameof(value))
        };

    public static IReadOnlyList<PlanLength> All => [OneWeek, TwoWeeks];

    public override string ToString() => Value == 1 ? "1 week" : "2 weeks";
}
