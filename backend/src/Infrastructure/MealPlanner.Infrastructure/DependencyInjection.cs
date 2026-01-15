using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryPlannedMealRepository>();
        services.AddSingleton<IPlannedMealRepository>(sp => sp.GetRequiredService<InMemoryPlannedMealRepository>());
        services.AddSingleton<IRecipeRepository>(sp => sp.GetRequiredService<InMemoryPlannedMealRepository>());
        return services;
    }
}
