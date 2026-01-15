using MediatR;
using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.DailyDigest;

public sealed record GetDailyDigestQuery(DateOnly Date) : IRequest<DailyDigestDto>;

public sealed class GetDailyDigestQueryHandler : IRequestHandler<GetDailyDigestQuery, DailyDigestDto>
{
    private readonly IPlannedMealRepository _repository;

    public GetDailyDigestQueryHandler(IPlannedMealRepository repository)
    {
        _repository = repository;
    }

    public async Task<DailyDigestDto> Handle(GetDailyDigestQuery request, CancellationToken cancellationToken)
    {
        var meals = await _repository.GetByDateAsync(request.Date, cancellationToken);

        var mealDtos = meals
            .OrderBy(m => GetMealTypeOrder(m.MealType))
            .Select(m => new PlannedMealDto(
                m.Id,
                m.MealType.Value,
                m.Recipe?.Name ?? "No recipe assigned",
                m.Recipe?.ImageUrl
            ))
            .ToList();

        return new DailyDigestDto(request.Date, mealDtos);
    }

    private static int GetMealTypeOrder(MealType mealType)
    {
        if (mealType == MealType.Breakfast) return 0;
        if (mealType == MealType.Lunch) return 1;
        if (mealType == MealType.Dinner) return 2;
        return 3;
    }
}
