using MediatR;
using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Application.ShoppingList;

public sealed record ToggleShoppingItemCommand(
    DateOnly StartDate,
    string ItemId,
    bool IsChecked
) : IRequest<Unit>;

public sealed class ToggleShoppingItemCommandHandler : IRequestHandler<ToggleShoppingItemCommand, Unit>
{
    private readonly IShoppingListStateRepository _stateRepository;

    public ToggleShoppingItemCommandHandler(IShoppingListStateRepository stateRepository)
    {
        _stateRepository = stateRepository;
    }

    public async Task<Unit> Handle(ToggleShoppingItemCommand request, CancellationToken cancellationToken)
    {
        var state = await _stateRepository.GetOrCreateAsync(request.StartDate, cancellationToken);
        state.SetItemChecked(request.ItemId, request.IsChecked);
        await _stateRepository.SaveAsync(state, cancellationToken);
        return Unit.Value;
    }
}
