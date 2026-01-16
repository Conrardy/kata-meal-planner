using MediatR;
using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.WeeklyPlan;

public sealed record GetWeeklyPlanQuery(DateOnly StartDate) : IRequest<WeeklyPlanDto>;

public sealed class GetWeeklyPlanQueryHandler : IRequestHandler<GetWeeklyPlanQuery, WeeklyPlanDto>
{
    private readonly IPlannedMealRepository _repository;

    public GetWeeklyPlanQueryHandler(IPlannedMealRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeeklyPlanDto> Handle(GetWeeklyPlanQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.StartDate.AddDays(6);
        var meals = await _repository.GetByDateRangeAsync(request.StartDate, endDate, cancellationToken);

        var days = new List<DayPlanDto>();
        for (var date = request.StartDate; date <= endDate; date = date.AddDays(1))
        {
            var dayMeals = meals.Where(m => m.Date == date).ToList();
            var dayPlan = new DayPlanDto(
                date,
                date.DayOfWeek.ToString(),
                MapMeal(dayMeals.FirstOrDefault(m => m.MealType == MealType.Breakfast)),
                MapMeal(dayMeals.FirstOrDefault(m => m.MealType == MealType.Lunch)),
                MapMeal(dayMeals.FirstOrDefault(m => m.MealType == MealType.Dinner))
            );
            days.Add(dayPlan);
        }

        return new WeeklyPlanDto(request.StartDate, endDate, days);
    }

    private static WeeklyMealDto? MapMeal(PlannedMeal? meal)
    {
        if (meal == null) return null;
        return new WeeklyMealDto(
            meal.Id,
            meal.RecipeId,
            meal.Recipe?.Name ?? "No recipe assigned",
            meal.Recipe?.ImageUrl
        );
    }
}
