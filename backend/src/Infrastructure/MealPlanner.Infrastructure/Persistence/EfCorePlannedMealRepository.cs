using MealPlanner.Domain.Meals;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class EfCorePlannedMealRepository : IPlannedMealRepository
{
    private readonly MealPlannerDbContext _context;

    public EfCorePlannedMealRepository(MealPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PlannedMeal>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedMeals
            .Include(m => m.Recipe)
            .Where(m => m.Date == date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlannedMeal>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedMeals
            .Include(m => m.Recipe)
            .Where(m => m.Date >= startDate && m.Date <= endDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlannedMeal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedMeals
            .Include(m => m.Recipe)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<PlannedMeal?> GetByDateAndMealTypeAsync(DateOnly date, MealType mealType, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedMeals
            .Include(m => m.Recipe)
            .FirstOrDefaultAsync(m => m.Date == date && m.MealType == mealType, cancellationToken);
    }

    public async Task UpdateAsync(PlannedMeal meal, CancellationToken cancellationToken = default)
    {
        _context.PlannedMeals.Update(meal);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(PlannedMeal meal, CancellationToken cancellationToken = default)
    {
        var existing = await _context.PlannedMeals
            .FirstOrDefaultAsync(m => m.Date == meal.Date && m.MealType == meal.MealType, cancellationToken);

        if (existing != null)
        {
            throw new InvalidOperationException(
                $"A meal already exists for {meal.Date:yyyy-MM-dd} {meal.MealType.Value}. " +
                $"Use UpdateAsync to modify the existing meal or remove it first.");
        }

        _context.PlannedMeals.Add(meal);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
