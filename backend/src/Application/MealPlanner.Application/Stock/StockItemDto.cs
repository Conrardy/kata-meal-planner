namespace MealPlanner.Application.Stock;

public sealed record StockItemDto(
    Guid Id,
    string IngredientName,
    decimal Quantity,
    string Unit,
    string Category,
    DateOnly? ExpirationDate,
    decimal? LowStockThreshold
);

public sealed record StockListDto(
    IReadOnlyList<StockItemDto> Items
);
