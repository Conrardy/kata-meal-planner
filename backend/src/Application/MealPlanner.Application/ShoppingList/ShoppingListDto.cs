namespace MealPlanner.Application.ShoppingList;

public sealed record ShoppingListDto(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<ShoppingCategoryDto> Categories
);

public sealed record ShoppingCategoryDto(
    string Category,
    IReadOnlyList<ShoppingItemDto> Items
);

public sealed record ShoppingItemDto(
    string Name,
    string Quantity,
    string? Unit
);
