using MealPlanner.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class EfCoreStockItemRepository : IStockItemRepository
{
    private readonly MealPlannerDbContext _context;

    public EfCoreStockItemRepository(MealPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StockItem>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.StockItems
            .Where(s => s.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<StockItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(StockItem item, CancellationToken cancellationToken = default)
    {
        await _context.StockItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StockItem item, CancellationToken cancellationToken = default)
    {
        _context.StockItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.StockItems.FindAsync([id], cancellationToken);
        if (item is not null)
        {
            _context.StockItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
