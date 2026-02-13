using ErrorOr;
using MealPlanner.Application.Common.Mediator;
using MealPlanner.Domain.Stock;

namespace MealPlanner.Application.Stock;

public sealed record UpdateStockItemCommand(
    Guid Id,
    Guid UserId,
    string IngredientName,
    decimal Quantity,
    string Unit,
    string Category,
    DateOnly? ExpirationDate,
    decimal? LowStockThreshold
) : IRequest<ErrorOr<StockItemDto>>, ITransactionalRequest;

public sealed class UpdateStockItemCommandHandler : IRequestHandler<UpdateStockItemCommand, ErrorOr<StockItemDto>>
{
    private readonly IStockItemRepository _repository;

    public UpdateStockItemCommandHandler(IStockItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<StockItemDto>> Handle(UpdateStockItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null || item.UserId != request.UserId)
            return Error.NotFound("StockItem.NotFound", $"Stock item {request.Id} not found.");

        var category = StockCategory.FromString(request.Category);
        item.Update(
            request.IngredientName,
            request.Quantity,
            request.Unit,
            category,
            request.ExpirationDate,
            request.LowStockThreshold
        );

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
