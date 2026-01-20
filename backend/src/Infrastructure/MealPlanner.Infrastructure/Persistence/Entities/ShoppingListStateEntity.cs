namespace MealPlanner.Infrastructure.Persistence;

public sealed class ShoppingListStateEntity
{
    public Guid Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Dictionary<string, bool> CheckedItems { get; set; } = new();
    public List<CustomItemData> CustomItems { get; set; } = [];
}

public sealed class CustomItemData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Category { get; set; } = "Pantry";
}
