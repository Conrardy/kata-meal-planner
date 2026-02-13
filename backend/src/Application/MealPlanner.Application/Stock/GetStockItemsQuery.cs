using MealPlanner.Application.Common.Mediator;
using MealPlanner.Domain.Stock;

namespace MealPlanner.Application.Stock;

public sealed record GetStockItemsQuery(Guid UserId) : IRequest<StockListDto>;

public sealed class GetStockItemsQueryHandler : IRequestHandler<GetStockItemsQuery, StockListDto>
{
    private readonly IStockItemRepository _repository;

    public GetStockItemsQueryHandler(IStockItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<StockListDto> Handle(GetStockItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllByUserAsync(request.UserId, cancellationToken);

        var dtos = items.Select(i => new StockItemDto(
            i.Id,
            i.IngredientName,
            i.Quantity,
            i.Unit,
            i.Category.Value,
            i.ExpirationDate,
            i.LowStockThreshold
        )).ToList();

        return new StockListDto(dtos);
    }
}
