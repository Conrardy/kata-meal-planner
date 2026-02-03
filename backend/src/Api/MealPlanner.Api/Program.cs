using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using ErrorOr;
using FluentValidation;
using HealthChecks.NpgSql;
using MealPlanner.Api.Configuration;
using MealPlanner.Api.Extensions;
using MealPlanner.Api.Logging;
using MealPlanner.Api.Middleware;
using MealPlanner.Application.Admin;
using MealPlanner.Application.Auth;
using MealPlanner.Application.Common.Behaviors;
using MealPlanner.Application.Common.Mediator;
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
using Microsoft.AspNetCore.RateLimiting;
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

    // Custom mediator with pipeline behaviors (replaces MediatR as of US-023)
    builder.Services.AddMediator(applicationAssembly);
    builder.Services.AddMediatorBehavior(typeof(LoggingBehavior<,>));
    builder.Services.AddMediatorBehavior(typeof(ValidationBehavior<,>));
    builder.Services.AddMediatorBehavior(typeof(TransactionBehavior<,>));

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

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdmin", policy =>
            policy.RequireClaim("IsAdmin", "true"));
    });

    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });

    if (!builder.Environment.IsDevelopment())
    {
        Log.Information(
            "Configuring HTTPS: HSTS enabled with MaxAge={MaxAge}days Preload={Preload} IncludeSubDomains={IncludeSubDomains}",
            365,
            true,
            true);
    }

    var rateLimitSettings = builder.Configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>()!;
    if (rateLimitSettings?.DefaultPolicy == null || rateLimitSettings.AuthPolicy == null)
    {
        throw new InvalidOperationException(
            $"Rate limiting settings are missing or invalid. Check configuration section '{RateLimitSettings.SectionName}'.");
    }

    Log.Information(
        "Configuring rate limiting: Default={DefaultLimit} req/{DefaultWindow}s, Auth={AuthLimit} req/{AuthWindow}s",
        rateLimitSettings.DefaultPolicy.PermitLimit,
        rateLimitSettings.DefaultPolicy.WindowInSeconds,
        rateLimitSettings.AuthPolicy.PermitLimit,
        rateLimitSettings.AuthPolicy.WindowInSeconds);

    builder.Services.AddRateLimiter(options =>
    {
        options.OnRejected = RateLimitingMiddleware.OnRejected;

        options.AddFixedWindowLimiter("default", limiterOptions =>
        {
            limiterOptions.PermitLimit = rateLimitSettings.DefaultPolicy.PermitLimit;
            limiterOptions.Window = TimeSpan.FromSeconds(rateLimitSettings.DefaultPolicy.WindowInSeconds);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = rateLimitSettings.DefaultPolicy.QueueLimit;
        });

        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = rateLimitSettings.AuthPolicy.PermitLimit;
            limiterOptions.Window = TimeSpan.FromSeconds(rateLimitSettings.AuthPolicy.WindowInSeconds);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = rateLimitSettings.AuthPolicy.QueueLimit;
        });

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var endpoint = context.Request.Path.Value ?? string.Empty;

            if (endpoint.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase))
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"auth:{clientIp}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitSettings.AuthPolicy.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitSettings.AuthPolicy.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitSettings.AuthPolicy.QueueLimit,
                    });
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"default:{clientIp}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitSettings.DefaultPolicy.PermitLimit,
                    Window = TimeSpan.FromSeconds(rateLimitSettings.DefaultPolicy.WindowInSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitSettings.DefaultPolicy.QueueLimit,
                });
        });
    });

    var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()!;
    if (corsSettings == null || corsSettings.AllowedOrigins.Count == 0)
    {
        throw new InvalidOperationException(
            $"CORS settings are missing or invalid. Check configuration section '{CorsSettings.SectionName}'.");
    }

    Log.Information(
        "Configuring CORS policy: PolicyName={PolicyName} AllowedOrigins={AllowedOrigins} AllowCredentials={AllowCredentials} PreflightMaxAge={PreflightMaxAge}s",
        corsSettings.PolicyName,
        string.Join(", ", corsSettings.AllowedOrigins),
        corsSettings.AllowCredentials,
        corsSettings.PreflightMaxAgeSeconds);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsSettings.PolicyName, policy =>
        {
            policy.WithOrigins([.. corsSettings.AllowedOrigins]);

            if (corsSettings.AllowedHeaders.Count > 0)
            {
                policy.WithHeaders([.. corsSettings.AllowedHeaders]);
            }
            else
            {
                policy.AllowAnyHeader();
            }

            if (corsSettings.AllowedMethods.Count > 0)
            {
                policy.WithMethods([.. corsSettings.AllowedMethods]);
            }
            else
            {
                policy.AllowAnyMethod();
            }

            if (corsSettings.AllowCredentials)
            {
                policy.AllowCredentials();
            }

            policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.PreflightMaxAgeSeconds));
        });
    });

    var postgresConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    Log.Information(
        "Configuring health checks. PostgresConfigured={PostgresConfigured}",
        !string.IsNullOrWhiteSpace(postgresConnectionString));

    builder.Services.AddHealthChecks()
        .AddNpgSql(
            connectionString: postgresConnectionString,
            name: "postgresql",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "sql", "postgresql", "ready"],
            timeout: TimeSpan.FromSeconds(5));

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
    app.UseCors(corsSettings.PolicyName);

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRateLimiter();

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
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/v1/auth/login", async (HttpContext httpContext, UsernameLoginRequest request, IMediator mediator) =>
{
    var command = new MealPlanner.Application.Auth.Login.LoginCommand(request.Username, request.Password);
    var result = await mediator.Send(command);
    return result.MatchResult(
        httpContext,
        response => Results.Ok(response));
})
.WithName("Login")
.WithOpenApi();

app.MapPost("/api/v1/auth/login/email", async (HttpContext httpContext, LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return result.MatchResult(
        httpContext,
        response => Results.Ok(response));
})
.WithName("LoginWithEmail")
.WithOpenApi();

app.MapPost("/api/v1/auth/refresh", async (HttpContext httpContext, UsernameRefreshTokenRequest request, IMediator mediator) =>
{
    var command = new MealPlanner.Application.Auth.Login.RefreshTokenCommand(request.RefreshToken);
    var result = await mediator.Send(command);
    return result.MatchResult(
        httpContext,
        response => Results.Ok(response));
})
.WithName("RefreshToken")
.WithOpenApi();

app.MapPost("/api/v1/auth/refresh/email", async (HttpContext httpContext, RefreshTokenRequest request, IAuthService authService) =>
{
    var result = await authService.RefreshTokenAsync(request);
    return result.MatchResult(
        httpContext,
        response => Results.Ok(response));
})
.WithName("RefreshTokenWithEmail")
.WithOpenApi();

app.MapGet("/api/v1/admin/users", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetUsersQuery());
    return Results.Ok(result);
})
.WithName("GetAdminUsers")
.WithOpenApi()
.RequireAuthorization("RequireAdmin");

app.MapPost("/api/v1/admin/users", async (HttpContext httpContext, CreateAdminUserRequest request, IMediator mediator) =>
{
    var command = new CreateUserCommand(request.Username, request.Password);
    var result = await mediator.Send(command);
    return result.MatchResult(
        httpContext,
        response => Results.Created($"/api/v1/admin/users/{response.UserId}", response));
})
.WithName("CreateAdminUser")
.WithOpenApi()
.RequireAuthorization("RequireAdmin");

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
    return result.Match(
        value => Results.Created($"/api/v1/meals/{value.MealId}", value),
        errors => Results.Problem(statusCode: errors[0].Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        }, title: errors[0].Description)
    );
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

public record CreateAdminUserRequest(string Username, string Password);

public record UsernameLoginRequest(string Username, string Password);
public record UsernameRefreshTokenRequest(string RefreshToken);
