using MealPlanner.Application.Auth;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Preferences;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.ShoppingList;
using MealPlanner.Infrastructure.Identity;
using MealPlanner.Infrastructure.Persistence;
using MealPlanner.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MealPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MealPlannerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<MealPlannerDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IRecipeRepository, EfCoreRecipeRepository>();
        services.AddScoped<IPlannedMealRepository, EfCorePlannedMealRepository>();
        services.AddScoped<IShoppingListStateRepository, EfCoreShoppingListStateRepository>();
        services.AddScoped<IUserPreferencesRepository, EfCoreUserPreferencesRepository>();
        services.AddScoped<DatabaseSeeder>();

        services.AddSingleton<IShoppingListSyncService, InMemoryShoppingListSyncService>();
        services.AddSingleton<IAllergenDetector, AllergenDetector>();
        services.AddSingleton<IQuantityParser, QuantityParser>();

        return services;
    }
}
