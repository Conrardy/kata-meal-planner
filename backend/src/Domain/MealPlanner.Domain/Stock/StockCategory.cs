namespace MealPlanner.Domain.Stock;

public sealed record StockCategory
{
    public static readonly StockCategory Produce = new("Produce");
    public static readonly StockCategory Dairy = new("Dairy");
    public static readonly StockCategory Meat = new("Meat");
    public static readonly StockCategory Pantry = new("Pantry");

    public string Value { get; }

    private StockCategory(string value) => Value = value;

    public static StockCategory FromString(string category)
    {
        return category switch
        {
            "Produce" or "produce" => Produce,
            "Dairy" or "dairy" => Dairy,
            "Meat" or "meat" => Meat,
            "Pantry" or "pantry" => Pantry,
            _ => throw new ArgumentException($"Invalid stock category: {category}. Must be one of: Produce, Dairy, Meat, Pantry.")
        };
    }

    public override string ToString() => Value;
}
