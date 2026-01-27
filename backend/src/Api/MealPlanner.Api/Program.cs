using System.Text;
using System.Text.Json;
using FluentValidation;
using HealthChecks.NpgSql;
using HealthChecks.Redis;
using MediatR;
using MealPlanner.Api.Extensions;
using MealPlanner.Api.Logging;
using MealPlanner.Api.Middleware;
using MealPlanner.Application.Auth;
using MealPlanner.Application.Common.Behaviors;
using MealPlanner.Application.DailyDigest;
using MealPlanner.Application.Meals;
using MealPlanner.Application.Preferences;
using MealPlanner.Application.Recipes;
using MealPlanner.Application.ShoppingList;
using MealPlanner.Application.WeeklyPlan;
using MealPlanner.Infrastructure;
using MealPlanner.Infrastructure.Identity;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MealPlanner API bootstrap");

    var builder = WebApplication.CreateBuilder(args);

    Log.Information(
        "Host bootstrap: ApplicationName={ApplicationName} Environment={Environment} ContentRoot={ContentRoot}",
        builder.Environment.ApplicationName,
        builder.Environment.EnvironmentName,
        builder.Environment.ContentRootPath);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithCorrelationIdHeader("X-Correlation-ID")
        .Destructure.With<SensitiveDataMaskingPolicy>());

    Log.Information("Configuring services");

    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddInfrastructure(builder.Configuration);

    var applicationAssembly = typeof(GetDailyDigestQuery).Assembly;
    builder.Services.AddValidatorsFromAssembly(applicationAssembly);
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(applicationAssembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
    if (string.IsNullOrWhiteSpace(jwtSettings.Issuer)
        || string.IsNullOrWhiteSpace(jwtSettings.Audience)
        || string.IsNullOrWhiteSpace(jwtSettings.Secret))
    {
        throw new InvalidOperationException(
            $"JWT settings are missing or invalid. Check configuration section '{JwtSettings.SectionName}' (Issuer, Audience, Secret). ");
    }

    var jwtSecretBytes = Encoding.UTF8.GetBytes(jwtSettings.Secret);
    Log.Information(
        "Configuring JWT authentication: Issuer={Issuer} Audience={Audience} SecretLength={SecretLength}",
        jwtSettings.Issuer,
        jwtSettings.Audience,
        jwtSettings.Secret.Length);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSecretBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    Log.Information("Configuring CORS default policy");
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    var postgresConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
    Log.Information(
        "Configuring health checks. PostgresConfigured={PostgresConfigured} RedisConfigured={RedisConfigured}",
        !string.IsNullOrWhiteSpace(postgresConnectionString),
        !string.IsNullOrWhiteSpace(redisConnectionString));

    builder.Services.AddHealthChecks()
        .AddNpgSql(
            connectionString: postgresConnectionString,
            name: "postgresql",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "sql", "postgresql", "ready"],
            timeout: TimeSpan.FromSeconds(5));
       /* .AddRedis(
            redisConnectionString: redisConnectionString,
            name: "redis",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["cache", "redis", "ready"],
            timeout: TimeSpan.FromSeconds(5));*/

    var app = builder.Build();
    Log.Information(
        "Application built. Environment={Environment} Urls={Urls}",
        app.Environment.EnvironmentName,
        string.Join(",", app.Urls));

if (app.Environment.IsDevelopment())
{
    Log.Information("Development mode enabled: OpenAPI + EF migrations + seeding");
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();
    try
    {
        Log.Information("Applying EF Core migrations");
        await dbContext.Database.MigrateAsync();

        Log.Information("Seeding database");

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();

        Log.Information("Database initialization completed");
    }
    catch (Npgsql.PostgresException pgEx)
    {
        throw new InvalidOperationException(
            "Database connection or authentication failed. Ensure the PostgreSQL server is running and the database 'mealplanner' exists with correct credentials.\n" +
            "For local development you can initialize the database using the project's docker-compose.yml (service 'postgres') or create the database manually.\n" +
            "Example (with docker): `docker compose up -d postgres` and then exec into the container to run psql.\n" +
            $"Original error: {pgEx.Message}",
            pgEx);
    }
}

    Log.Information("Configuring middleware pipeline");

    app.UseCorrelationId();
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.HasValue ? httpContext.Request.Host.Value : string.Empty);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? string.Empty);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString() ?? string.Empty);
            if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                diagnosticContext.Set("CorrelationId", correlationId?.ToString() ?? string.Empty);
            }
        };
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });
    app.UseExceptionHandler();
    app.UseCors();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    Log.Information("Mapping endpoints");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthCheckResponse
})
.AllowAnonymous()
.WithName("LivenessCheck")
.WithOpenApi();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
})
.AllowAnonymous()
.WithName("ReadinessCheck")
.WithOpenApi();

app.MapPost("/api/v1/auth/register", async (HttpContext httpContext, RegisterRequest request, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    return result.MatchResult(
        httpContext,
        response => Results.Created($"/api/v1/users/{response.UserId}", response));
})
.WithName("Register")
.WithOpenApi();

app.MapPost("/api/v1/auth/login", async (HttpContext httpContext, LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return result.MatchResult(
        httpContext,
        response => Results.Ok(response));
})
.WithName("Login")
.WithOpenApi();

app.MapPost("/api/v1/auth/refresh", async (HttpContext httpContext, RefreshTokenRequest request, IAuthService authService) =>
{
    var result = await authService.RefreshTokenAsync(request);
    return result.MatchResult(
        httpContext,
        response => Results.Ok(response));
})
.WithName("RefreshToken")
.WithOpenApi();

app.MapGet("/api/v1/daily-digest/{date}", async (DateOnly date, IMediator mediator) =>
{
    var query = new GetDailyDigestQuery(date);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetDailyDigest")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/v1/meals/{mealId}/suggestions", async (HttpContext httpContext, Guid mealId, IMediator mediator) =>
{
    var query = new GetSuggestionsQuery(mealId);
    var result = await mediator.Send(query);
    return result.MatchResult(httpContext, value => Results.Ok(value));
})
.WithName("GetMealSuggestions")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/v1/meals/{mealId}/swap", async (HttpContext httpContext, Guid mealId, SwapMealRequest request, IMediator mediator) =>
{
    var command = new SwapMealCommand(mealId, request.NewRecipeId);
    var result = await mediator.Send(command);
    return result.MatchResult(httpContext, value => Results.Ok(value));
})
.WithName("SwapMeal")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/v1/recipes/{recipeId}", async (HttpContext httpContext, Guid recipeId, IMediator mediator) =>
{
    var query = new GetRecipeDetailsQuery(recipeId);
    var result = await mediator.Send(query);
    if (result is null)
    {
        var problemDetails = ApiProblemDetailsFactory.CreateProblemDetails(
            httpContext,
            StatusCodes.Status404NotFound,
            "Not Found",
            $"Recipe with ID '{recipeId}' was not found.");
        return Results.Problem(problemDetails);
    }
    return Results.Ok(result);
})
.WithName("GetRecipeDetails")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/v1/weekly-plan/{startDate}", async (DateOnly startDate, IMediator mediator) =>
{
    var query = new GetWeeklyPlanQuery(startDate);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetWeeklyPlan")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/v1/recipes", async (string? search, string? tags, IMediator mediator) =>
{
    var tagList = string.IsNullOrWhiteSpace(tags)
        ? null
        : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    var query = new SearchRecipesQuery(search, tagList);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("SearchRecipes")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/v1/recipes", async (CreateRecipeRequest request, IMediator mediator) =>
{
    var command = new CreateRecipeCommand(
        request.Name,
        request.ImageUrl,
        request.Description,
        request.Ingredients.Select(i => new CreateIngredientDto(i.Name, i.Quantity, i.Unit)).ToList(),
        request.Steps.Select(s => new CreateCookingStepDto(s.StepNumber, s.Instruction)).ToList(),
        request.Tags,
        request.MealType
    );

    var id = await mediator.Send(command);
    return Results.Created($"/api/v1/recipes/{id}", new { Id = id });
})
.WithName("CreateRecipe")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/v1/meal-plan", async (AddRecipeToMealPlanRequest request, IMediator mediator) =>
{
    var command = new AddRecipeToMealPlanCommand(
        request.RecipeId,
        DateOnly.Parse(request.Date),
        request.MealType
    );
    var result = await mediator.Send(command);
    return Results.Created($"/api/v1/meals/{result.MealId}", result);
})
.WithName("AddRecipeToMealPlan")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/v1/shopping-list/{startDate}", async (DateOnly startDate, IMediator mediator) =>
{
    var query = new GenerateShoppingListQuery(startDate);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GenerateShoppingList")
.WithOpenApi()
.RequireAuthorization();

app.MapPatch("/api/v1/shopping-list/{startDate}/items/{itemId}", async (DateOnly startDate, string itemId, ToggleItemRequest request, IMediator mediator) =>
{
    var command = new ToggleShoppingItemCommand(startDate, itemId, request.IsChecked);
    await mediator.Send(command);
    return Results.NoContent();
})
.WithName("ToggleShoppingItem")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/v1/shopping-list/{startDate}/items", async (DateOnly startDate, AddCustomItemRequest request, IMediator mediator) =>
{
    var command = new AddCustomItemCommand(startDate, request.Name, request.Quantity, request.Unit, request.Category);
    var result = await mediator.Send(command);
    return Results.Created($"/api/v1/shopping-list/{startDate}/items/{result.Id}", result);
})
.WithName("AddCustomItem")
.WithOpenApi()
.RequireAuthorization();

app.MapDelete("/api/v1/shopping-list/{startDate}/items/{itemId}", async (HttpContext httpContext, DateOnly startDate, string itemId, IMediator mediator) =>
{
    var command = new RemoveShoppingItemCommand(startDate, itemId);
    var removed = await mediator.Send(command);
    if (!removed)
    {
        var problemDetails = ApiProblemDetailsFactory.CreateProblemDetails(
            httpContext,
            StatusCodes.Status404NotFound,
            "Not Found",
            $"Shopping item with ID '{itemId}' was not found.");
        return Results.Problem(problemDetails);
    }
    return Results.NoContent();
})
.WithName("RemoveShoppingItem")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/v1/preferences", async (IMediator mediator) =>
{
    var query = new GetUserPreferencesQuery();
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetUserPreferences")
.WithOpenApi()
.RequireAuthorization();

app.MapPut("/api/v1/preferences", async (UpdatePreferencesRequest request, IMediator mediator) =>
{
    var command = new UpdateUserPreferencesCommand(
        request.DietaryPreference,
        request.Allergies,
        request.MealsPerDay,
        request.PlanLength,
        request.IncludeLeftovers,
        request.AutoGenerateShoppingList,
        request.ExcludedIngredients
    );
    var result = await mediator.Send(command);
    return Results.Ok(result);
})
.WithName("UpdateUserPreferences")
.WithOpenApi()
.RequireAuthorization();

    Log.Information("Initialization complete. Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            duration = entry.Value.Duration.TotalMilliseconds,
            description = entry.Value.Description,
            exception = entry.Value.Exception?.Message,
            data = entry.Value.Data.Count > 0 ? entry.Value.Data : null
        })
    };

    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    return context.Response.WriteAsJsonAsync(response, options);
}

public record SwapMealRequest(Guid NewRecipeId);
public record AddRecipeToMealPlanRequest(Guid RecipeId, string Date, string MealType);
public record ToggleItemRequest(bool IsChecked);
public record AddCustomItemRequest(string Name, string Quantity, string? Unit, string Category);
public record UpdatePreferencesRequest(
    string DietaryPreference,
    List<string> Allergies,
    int? MealsPerDay = null,
    int? PlanLength = null,
    bool? IncludeLeftovers = null,
    bool? AutoGenerateShoppingList = null,
    List<string>? ExcludedIngredients = null
);
