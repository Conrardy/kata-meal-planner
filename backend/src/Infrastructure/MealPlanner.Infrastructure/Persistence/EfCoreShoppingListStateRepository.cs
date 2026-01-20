using MealPlanner.Domain.ShoppingList;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class EfCoreShoppingListStateRepository : IShoppingListStateRepository
{
    private readonly MealPlannerDbContext _context;

    public EfCoreShoppingListStateRepository(MealPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<ShoppingListState> GetOrCreateAsync(DateOnly startDate, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ShoppingListStates
            .FirstOrDefaultAsync(s => s.StartDate == startDate, cancellationToken);

        var state = new ShoppingListState(startDate);

        if (entity is not null)
        {
            foreach (var (itemId, isChecked) in entity.CheckedItems)
            {
                state.SetItemChecked(itemId, isChecked);
            }

            foreach (var item in entity.CustomItems)
            {
                var category = ItemCategory.FromIngredient(item.Category);
                state.AddCustomItemWithId(item.Id, item.Name, item.Quantity, item.Unit, category);
            }
        }

        return state;
    }

    public async Task SaveAsync(ShoppingListState state, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ShoppingListStates
            .FirstOrDefaultAsync(s => s.StartDate == state.StartDate, cancellationToken);

        if (entity is null)
        {
            entity = new ShoppingListStateEntity
            {
                Id = Guid.NewGuid(),
                StartDate = state.StartDate,
                EndDate = state.EndDate
            };
            _context.ShoppingListStates.Add(entity);
        }

        entity.CheckedItems = new Dictionary<string, bool>(state.CheckedItems);
        entity.CustomItems = state.CustomItems.Select(item => new CustomItemData
        {
            Id = item.Id,
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            Category = item.Category.Value
        }).ToList();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
