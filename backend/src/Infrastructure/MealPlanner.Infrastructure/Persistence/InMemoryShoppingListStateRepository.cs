using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class InMemoryShoppingListStateRepository : IShoppingListStateRepository
{
    private readonly Dictionary<DateOnly, ShoppingListState> _states = new();

    public Task<ShoppingListState> GetOrCreateAsync(DateOnly startDate, CancellationToken cancellationToken = default)
    {
        if (!_states.TryGetValue(startDate, out var state))
        {
            state = new ShoppingListState(startDate);
            _states[startDate] = state;
        }
        return Task.FromResult(state);
    }

    public Task SaveAsync(ShoppingListState state, CancellationToken cancellationToken = default)
    {
        _states[state.StartDate] = state;
        return Task.CompletedTask;
    }
}
