using ErrorOr;
using MealPlanner.Application.Common.Mediator;
using MealPlanner.Domain.Stock;

namespace MealPlanner.Application.Stock;

public sealed record AdjustStockQuantityCommand(
    Guid Id,
    Guid UserId,
    decimal Adjustment
) : IRequest<ErrorOr<StockItemDto>>, ITransactionalRequest;

public sealed class AdjustStockQuantityCommandHandler : IRequestHandler<AdjustStockQuantityCommand, ErrorOr<StockItemDto>>
{
    private readonly IStockItemRepository _repository;

    public AdjustStockQuantityCommandHandler(IStockItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<StockItemDto>> Handle(AdjustStockQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null || item.UserId != request.UserId)
            return Error.NotFound("StockItem.NotFound", $"Stock item {request.Id} not found.");

        item.AdjustQuantity(request.Adjustment);
        await _repository.UpdateAsync(item, cancellationToken);

        return new StockItemDto(
            item.Id,
            item.IngredientName,
            item.Quantity,
            item.Unit,
            item.Category.Value,
            item.ExpirationDate,
            item.LowStockThreshold
        );
    }
}
