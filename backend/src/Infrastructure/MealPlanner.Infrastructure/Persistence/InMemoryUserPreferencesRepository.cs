using MealPlanner.Domain.Preferences;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class InMemoryUserPreferencesRepository : IUserPreferencesRepository
{
    private UserPreferences _preferences = new();

    public Task<UserPreferences> GetAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_preferences);
    }

    public Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
    {
        _preferences = preferences;
        return Task.CompletedTask;
    }
}
