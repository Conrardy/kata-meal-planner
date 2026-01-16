using MediatR;
using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Application.ShoppingList;

public sealed record RemoveShoppingItemCommand(
    DateOnly StartDate,
    string ItemId
) : IRequest<bool>;

public sealed class RemoveShoppingItemCommandHandler : IRequestHandler<RemoveShoppingItemCommand, bool>
{
    private readonly IShoppingListStateRepository _stateRepository;

    public RemoveShoppingItemCommandHandler(IShoppingListStateRepository stateRepository)
    {
        _stateRepository = stateRepository;
    }

    public async Task<bool> Handle(RemoveShoppingItemCommand request, CancellationToken cancellationToken)
    {
        var state = await _stateRepository.GetOrCreateAsync(request.StartDate, cancellationToken);
        var removed = state.RemoveItem(request.ItemId);
        await _stateRepository.SaveAsync(state, cancellationToken);
        return removed;
    }
}
