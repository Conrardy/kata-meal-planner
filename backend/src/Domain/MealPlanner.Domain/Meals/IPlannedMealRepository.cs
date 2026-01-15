namespace MealPlanner.Domain.Meals;

public interface IPlannedMealRepository
{
    Task<IReadOnlyList<PlannedMeal>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<PlannedMeal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlannedMeal meal, CancellationToken cancellationToken = default);
}
