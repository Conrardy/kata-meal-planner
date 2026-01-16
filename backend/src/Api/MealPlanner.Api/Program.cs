using MediatR;
using MealPlanner.Application.DailyDigest;
using MealPlanner.Application.Meals;
using MealPlanner.Application.Recipes;
using MealPlanner.Application.WeeklyPlan;
using MealPlanner.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure();
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

app.Run();

public record SwapMealRequest(Guid NewRecipeId);
