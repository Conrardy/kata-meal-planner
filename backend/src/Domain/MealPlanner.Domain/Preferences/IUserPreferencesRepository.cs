namespace MealPlanner.Domain.Preferences;

public interface IUserPreferencesRepository
{
    Task<UserPreferences> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default);
}
