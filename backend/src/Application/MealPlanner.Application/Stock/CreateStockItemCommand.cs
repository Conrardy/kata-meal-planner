using ErrorOr;
using MealPlanner.Application.Common.Mediator;
using MealPlanner.Domain.Stock;

namespace MealPlanner.Application.Stock;

public sealed record CreateStockItemCommand(
    Guid UserId,
    string IngredientName,
    decimal Quantity,
    string Unit,
    string Category,
    DateOnly? ExpirationDate,
    decimal? LowStockThreshold
) : IRequest<ErrorOr<StockItemDto>>, ITransactionalRequest;

public sealed class CreateStockItemCommandHandler : IRequestHandler<CreateStockItemCommand, ErrorOr<StockItemDto>>
{
    private readonly IStockItemRepository _repository;

    public CreateStockItemCommandHandler(IStockItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<StockItemDto>> Handle(CreateStockItemCommand request, CancellationToken cancellationToken)
    {
        var category = StockCategory.FromString(request.Category);
        var item = new StockItem(
            Guid.NewGuid(),
            request.IngredientName,
            request.Quantity,
            request.Unit,
            category,
            request.ExpirationDate,
            request.LowStockThreshold,
            request.UserId
        );

        await _repository.AddAsync(item, cancellationToken);

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
