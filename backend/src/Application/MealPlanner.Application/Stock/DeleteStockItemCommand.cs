using ErrorOr;
using MealPlanner.Application.Common.Mediator;
using MealPlanner.Domain.Stock;

namespace MealPlanner.Application.Stock;

public sealed record DeleteStockItemCommand(
    Guid Id,
    Guid UserId
) : IRequest<ErrorOr<Unit>>, ITransactionalRequest;

public sealed class DeleteStockItemCommandHandler : IRequestHandler<DeleteStockItemCommand, ErrorOr<Unit>>
{
    private readonly IStockItemRepository _repository;

    public DeleteStockItemCommandHandler(IStockItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<Unit>> Handle(DeleteStockItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null || item.UserId != request.UserId)
            return Error.NotFound("StockItem.NotFound", $"Stock item {request.Id} not found.");

        await _repository.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
