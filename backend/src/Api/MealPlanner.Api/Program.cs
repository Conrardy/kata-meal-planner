using MediatR;
using MealPlanner.Application.DailyDigest;
using MealPlanner.Application.Meals;
using MealPlanner.Application.Preferences;
using MealPlanner.Application.Recipes;
using MealPlanner.Application.ShoppingList;
using MealPlanner.Application.WeeklyPlan;
using MealPlanner.Infrastructure;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetDailyDigestQuery).Assembly));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseCors();
app.UseHttpsRedirection();

app.MapGet("/api/v1/daily-digest/{date}", async (DateOnly date, IMediator mediator) =>
{
    var query = new GetDailyDigestQuery(date);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetDailyDigest")
.WithOpenApi();

app.MapGet("/api/v1/meals/{mealId}/suggestions", async (Guid mealId, IMediator mediator) =>
{
    var query = new GetSuggestionsQuery(mealId);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetMealSuggestions")
.WithOpenApi();

app.MapPost("/api/v1/meals/{mealId}/swap", async (Guid mealId, SwapMealRequest request, IMediator mediator) =>
{
    var command = new SwapMealCommand(mealId, request.NewRecipeId);
    var result = await mediator.Send(command);
    return Results.Ok(result);
})
.WithName("SwapMeal")
.WithOpenApi();

app.MapGet("/api/v1/recipes/{recipeId}", async (Guid recipeId, IMediator mediator) =>
{
    var query = new GetRecipeDetailsQuery(recipeId);
    var result = await mediator.Send(query);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithName("GetRecipeDetails")
.WithOpenApi();

app.MapGet("/api/v1/weekly-plan/{startDate}", async (DateOnly startDate, IMediator mediator) =>
{
    var query = new GetWeeklyPlanQuery(startDate);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetWeeklyPlan")
.WithOpenApi();

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
.WithOpenApi();

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
.WithOpenApi();

app.MapGet("/api/v1/shopping-list/{startDate}", async (DateOnly startDate, IMediator mediator) =>
{
    var query = new GenerateShoppingListQuery(startDate);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GenerateShoppingList")
.WithOpenApi();

app.MapPatch("/api/v1/shopping-list/{startDate}/items/{itemId}", async (DateOnly startDate, string itemId, ToggleItemRequest request, IMediator mediator) =>
{
    var command = new ToggleShoppingItemCommand(startDate, itemId, request.IsChecked);
    await mediator.Send(command);
    return Results.NoContent();
})
.WithName("ToggleShoppingItem")
.WithOpenApi();

app.MapPost("/api/v1/shopping-list/{startDate}/items", async (DateOnly startDate, AddCustomItemRequest request, IMediator mediator) =>
{
    var command = new AddCustomItemCommand(startDate, request.Name, request.Quantity, request.Unit, request.Category);
    var result = await mediator.Send(command);
    return Results.Created($"/api/v1/shopping-list/{startDate}/items/{result.Id}", result);
})
.WithName("AddCustomItem")
.WithOpenApi();

app.MapDelete("/api/v1/shopping-list/{startDate}/items/{itemId}", async (DateOnly startDate, string itemId, IMediator mediator) =>
{
    var command = new RemoveShoppingItemCommand(startDate, itemId);
    var removed = await mediator.Send(command);
    return removed ? Results.NoContent() : Results.NotFound();
})
.WithName("RemoveShoppingItem")
.WithOpenApi();

app.MapGet("/api/v1/preferences", async (IMediator mediator) =>
{
    var query = new GetUserPreferencesQuery();
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("GetUserPreferences")
.WithOpenApi();

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
.WithOpenApi();

app.Run();

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
