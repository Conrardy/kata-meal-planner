using MediatR;
using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Application.ShoppingList;

public sealed record AddCustomItemCommand(
    DateOnly StartDate,
    string Name,
    string Quantity,
    string? Unit,
    string Category
) : IRequest<ShoppingItemDto>;

public sealed class AddCustomItemCommandHandler : IRequestHandler<AddCustomItemCommand, ShoppingItemDto>
{
    private readonly IShoppingListStateRepository _stateRepository;

    public AddCustomItemCommandHandler(IShoppingListStateRepository stateRepository)
    {
        _stateRepository = stateRepository;
    }

    public async Task<ShoppingItemDto> Handle(AddCustomItemCommand request, CancellationToken cancellationToken)
    {
        var state = await _stateRepository.GetOrCreateAsync(request.StartDate, cancellationToken);
        var category = ItemCategory.FromString(request.Category);
        var item = state.AddCustomItem(request.Name, request.Quantity, request.Unit, category);
        await _stateRepository.SaveAsync(state, cancellationToken);

        return new ShoppingItemDto(
            Id: item.Id,
            Name: item.Name,
            Quantity: item.Quantity,
            Unit: item.Unit,
            IsChecked: item.IsChecked,
            IsCustom: item.IsCustom
        );
    }
}
