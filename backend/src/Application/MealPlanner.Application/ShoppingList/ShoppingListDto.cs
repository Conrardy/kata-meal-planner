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
    string Id,
    string Name,
    string Quantity,
    string? Unit,
    bool IsChecked,
    bool IsCustom
);

public sealed record AddCustomItemDto(
    string Name,
    string Quantity,
    string? Unit,
    string Category
);

public sealed record ToggleItemDto(
    bool IsChecked
);
