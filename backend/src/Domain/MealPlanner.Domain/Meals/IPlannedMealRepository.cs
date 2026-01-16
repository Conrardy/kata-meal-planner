namespace MealPlanner.Domain.Meals;

public interface IPlannedMealRepository
{
    Task<IReadOnlyList<PlannedMeal>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlannedMeal>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<PlannedMeal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PlannedMeal?> GetByDateAndMealTypeAsync(DateOnly date, MealType mealType, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlannedMeal meal, CancellationToken cancellationToken = default);
    Task AddAsync(PlannedMeal meal, CancellationToken cancellationToken = default);
}
