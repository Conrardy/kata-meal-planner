using MealPlanner.Domain.Meals;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPlannedMealRepository, InMemoryPlannedMealRepository>();
        return services;
    }
}
