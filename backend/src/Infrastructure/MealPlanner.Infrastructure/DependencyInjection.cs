using MealPlanner.Application.Admin;
using MealPlanner.Application.Auth;
using MealPlanner.Application.Common.Interfaces;
using MealPlanner.Domain.Auth;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Preferences;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.ShoppingList;
using MealPlanner.Domain.Stock;
using MealPlanner.Infrastructure.Auth;
using MealPlanner.Infrastructure.Admin;
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

        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

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

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
            options.Cookie.IsEssential = true;
        });

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SeededUsersSettings>(configuration.GetSection(SeededUsersSettings.SectionName));
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        services.AddSingleton<ISeededUserRepository, SeededUserRepository>();
        services.AddScoped<ISeededUserAuthService, SeededUserAuthService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IPasswordHasher<DynamicUser>, PasswordHasher<DynamicUser>>();

        services.AddScoped<IRecipeRepository, EfCoreRecipeRepository>();
        services.AddScoped<IPlannedMealRepository, EfCorePlannedMealRepository>();
        services.AddScoped<IShoppingListStateRepository, EfCoreShoppingListStateRepository>();
        services.AddScoped<IUserPreferencesRepository, EfCoreUserPreferencesRepository>();
        services.AddScoped<IStockItemRepository, EfCoreStockItemRepository>();
        services.AddScoped<DatabaseSeeder>();

        services.AddSingleton<IShoppingListSyncService, InMemoryShoppingListSyncService>();
        services.AddSingleton<IAllergenDetector, AllergenDetector>();
        services.AddSingleton<IQuantityParser, QuantityParser>();

        return services;
    }
}
