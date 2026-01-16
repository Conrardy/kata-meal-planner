namespace MealPlanner.Domain.ShoppingList;

public sealed record ItemCategory
{
    public static readonly ItemCategory Produce = new("Produce");
    public static readonly ItemCategory Dairy = new("Dairy");
    public static readonly ItemCategory Meat = new("Meat");
    public static readonly ItemCategory Pantry = new("Pantry");

    public string Value { get; }

    private ItemCategory(string value) => Value = value;

    public static ItemCategory FromIngredient(string ingredientName)
    {
        var name = ingredientName.ToLowerInvariant();

        if (IsProduce(name)) return Produce;
        if (IsDairy(name)) return Dairy;
        if (IsMeat(name)) return Meat;
        return Pantry;
    }

    private static bool IsProduce(string name) =>
        name.Contains("lettuce") ||
        name.Contains("tomato") ||
        name.Contains("cucumber") ||
        name.Contains("onion") ||
        name.Contains("garlic") ||
        name.Contains("berries") ||
        name.Contains("berry") ||
        name.Contains("avocado") ||
        name.Contains("lemon") ||
        name.Contains("asparagus") ||
        name.Contains("broccoli") ||
        name.Contains("pepper") && !name.Contains("pepper flakes") ||
        name.Contains("ginger") ||
        name.Contains("dill") ||
        name.Contains("greens") ||
        name.Contains("fruit");

    private static bool IsDairy(string name) =>
        name.Contains("milk") ||
        name.Contains("cheese") ||
        name.Contains("parmesan") ||
        name.Contains("butter") ||
        name.Contains("yogurt") ||
        name.Contains("cream") ||
        name.Contains("egg");

    private static bool IsMeat(string name) =>
        name.Contains("chicken") ||
        name.Contains("beef") ||
        name.Contains("salmon") ||
        name.Contains("fish") ||
        name.Contains("pork") ||
        name.Contains("meat") ||
        name.Contains("tofu");

    public override string ToString() => Value;
}
