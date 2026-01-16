namespace MealPlanner.Domain.ShoppingList;

public sealed record ShoppingItem(
    string Id,
    string Name,
    string Quantity,
    string? Unit,
    ItemCategory Category,
    bool IsChecked = false,
    bool IsCustom = false
);
