using MealPlanner.Domain.Preferences;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class EfCoreUserPreferencesRepository : IUserPreferencesRepository
{
    private static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private readonly MealPlannerDbContext _context;

    public EfCoreUserPreferencesRepository(MealPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<UserPreferences> GetAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.Id == DefaultUserId, cancellationToken);

        if (entity is null)
        {
            return new UserPreferences();
        }

        return new UserPreferences(
            DietaryPreference.FromString(entity.DietaryPreference),
            entity.Allergies.Select(Allergy.FromString).ToList(),
            MealsPerDay.FromInt(entity.MealsPerDay),
            PlanLength.FromInt(entity.PlanLength),
            entity.IncludeLeftovers,
            entity.AutoGenerateShoppingList,
            entity.ExcludedIngredients
        );
    }

    public async Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.Id == DefaultUserId, cancellationToken);

        if (entity is null)
        {
            entity = new UserPreferencesEntity { Id = DefaultUserId };
            _context.UserPreferences.Add(entity);
        }

        entity.DietaryPreference = preferences.DietaryPreference.Value;
        entity.Allergies = preferences.Allergies.Select(a => a.Value).ToList();
        entity.MealsPerDay = preferences.MealsPerDay.Value;
        entity.PlanLength = preferences.PlanLength.Value;
        entity.IncludeLeftovers = preferences.IncludeLeftovers;
        entity.AutoGenerateShoppingList = preferences.AutoGenerateShoppingList;
        entity.ExcludedIngredients = preferences.ExcludedIngredients.ToList();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
