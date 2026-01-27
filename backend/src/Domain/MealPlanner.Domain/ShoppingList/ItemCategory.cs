namespace MealPlanner.Domain.ShoppingList;

public sealed record ItemCategory
{
    public static readonly ItemCategory Produce = new("Produce");
    public static readonly ItemCategory Dairy = new("Dairy");
    public static readonly ItemCategory Meat = new("Meat");
    public static readonly ItemCategory Pantry = new("Pantry");

    private static readonly string[] ProduceKeywords =
    [
        "lettuce", "tomato", "cucumber", "onion", "garlic",
        "berries", "berry", "avocado", "lemon", "asparagus",
        "broccoli", "ginger", "dill", "greens", "fruit"
    ];

    private static readonly string[] DairyKeywords =
    [
        "milk", "cheese", "parmesan", "butter", "yogurt", "cream", "egg"
    ];

    private static readonly string[] MeatKeywords =
    [
        "chicken", "beef", "salmon", "fish", "pork", "meat", "tofu"
    ];

    public string Value { get; }

    private ItemCategory(string value) => Value = value;

    public static ItemCategory FromString(string category)
    {
        return category switch
        {
            "Produce" or "produce" => Produce,
            "Dairy" or "dairy" => Dairy,
            "Meat" or "meat" => Meat,
            "Pantry" or "pantry" => Pantry,
            _ => Pantry
        };
    }

    public static ItemCategory FromIngredient(string ingredientName)
    {
        var name = ingredientName.ToLowerInvariant();

        if (IsProduce(name)) return Produce;
        if (IsDairy(name)) return Dairy;
        if (IsMeat(name)) return Meat;
        return Pantry;
    }

    private static bool IsProduce(string name) =>
        name.Contains("pepper") && !name.Contains("pepper flakes") ||
        ProduceKeywords.Any(keyword => name.Contains(keyword));

    private static bool IsDairy(string name) =>
        DairyKeywords.Any(keyword => name.Contains(keyword));

    private static bool IsMeat(string name) =>
        MeatKeywords.Any(keyword => name.Contains(keyword));

    public override string ToString() => Value;
}
