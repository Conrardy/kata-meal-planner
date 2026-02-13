namespace MealPlanner.Domain.Stock;

public sealed class StockItem
{
    public Guid Id { get; private set; }
    public string IngredientName { get; private set; }
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; }
    public StockCategory Category { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public decimal? LowStockThreshold { get; private set; }
    public Guid UserId { get; private set; }

    private StockItem() { IngredientName = string.Empty; Unit = string.Empty; Category = StockCategory.Pantry; }

    public StockItem(
        Guid id,
        string ingredientName,
        decimal quantity,
        string unit,
        StockCategory category,
        DateOnly? expirationDate,
        decimal? lowStockThreshold,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
            throw new ArgumentException("Ingredient name cannot be empty.");

        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be empty.");

        if (lowStockThreshold.HasValue && lowStockThreshold.Value < 0)
            throw new ArgumentException("Low stock threshold cannot be negative.");

        Id = id;
        IngredientName = ingredientName;
        Quantity = quantity;
        Unit = unit;
        Category = category;
        ExpirationDate = expirationDate;
        LowStockThreshold = lowStockThreshold;
        UserId = userId;
    }

    public void Update(
        string ingredientName,
        decimal quantity,
        string unit,
        StockCategory category,
        DateOnly? expirationDate,
        decimal? lowStockThreshold)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
            throw new ArgumentException("Ingredient name cannot be empty.");

        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be empty.");

        if (lowStockThreshold.HasValue && lowStockThreshold.Value < 0)
            throw new ArgumentException("Low stock threshold cannot be negative.");

        IngredientName = ingredientName;
        Quantity = quantity;
        Unit = unit;
        Category = category;
        ExpirationDate = expirationDate;
        LowStockThreshold = lowStockThreshold;
    }

    public void AdjustQuantity(decimal adjustment)
    {
        var newQuantity = Quantity + adjustment;
        if (newQuantity < 0)
            throw new InvalidOperationException("Quantity cannot be negative after adjustment.");

        Quantity = newQuantity;
    }
}
