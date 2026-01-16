namespace MealPlanner.Domain.ShoppingList;

public sealed record ShoppingItem(
    string Name,
    string Quantity,
    string? Unit,
    ItemCategory Category
);
