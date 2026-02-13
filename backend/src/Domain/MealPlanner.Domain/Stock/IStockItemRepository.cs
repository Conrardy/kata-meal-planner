namespace MealPlanner.Domain.Stock;

public interface IStockItemRepository
{
    Task<IReadOnlyList<StockItem>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<StockItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(StockItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(StockItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
