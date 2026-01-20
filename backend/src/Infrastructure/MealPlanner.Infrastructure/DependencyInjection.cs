using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Preferences;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.ShoppingList;
using MealPlanner.Infrastructure.Persistence;
using MealPlanner.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MealPlannerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IRecipeRepository, EfCoreRecipeRepository>();
        services.AddScoped<IPlannedMealRepository, EfCorePlannedMealRepository>();
        services.AddScoped<IShoppingListStateRepository, EfCoreShoppingListStateRepository>();
        services.AddScoped<IUserPreferencesRepository, EfCoreUserPreferencesRepository>();
        services.AddScoped<DatabaseSeeder>();

        services.AddSingleton<IShoppingListSyncService, InMemoryShoppingListSyncService>();

        return services;
    }
}
