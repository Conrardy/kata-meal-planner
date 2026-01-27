using MediatR;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Application.Meals;

public sealed record AddRecipeToMealPlanCommand(
    Guid RecipeId,
    DateOnly Date,
    string MealType
) : IRequest<AddRecipeToMealPlanResultDto>;

public sealed record AddRecipeToMealPlanResultDto(
    Guid MealId,
    string RecipeName,
    string Date,
    string MealType,
    bool ShoppingListUpdated = true
);

public sealed class AddRecipeToMealPlanCommandHandler : IRequestHandler<AddRecipeToMealPlanCommand, AddRecipeToMealPlanResultDto>
{
    private readonly IPlannedMealRepository _mealRepository;
    private readonly IShoppingListSyncService _syncService;

    public AddRecipeToMealPlanCommandHandler(
        IPlannedMealRepository mealRepository,
        IShoppingListSyncService syncService)
    {
        _mealRepository = mealRepository;
        _syncService = syncService;
    }

    public async Task<AddRecipeToMealPlanResultDto> Handle(AddRecipeToMealPlanCommand request, CancellationToken cancellationToken)
    {
        var mealType = MealType.FromString(request.MealType);

        var existingMeal = await _mealRepository.GetByDateAndMealTypeAsync(request.Date, mealType, cancellationToken);
        if (existingMeal != null)
        {
            existingMeal.SwapRecipe(request.RecipeId);
            await _mealRepository.UpdateAsync(existingMeal, cancellationToken);

            _syncService.MarkPendingSync(request.Date);

            return new AddRecipeToMealPlanResultDto(
                existingMeal.Id,
                existingMeal.Recipe?.Name ?? "Recipe",
                request.Date.ToString("yyyy-MM-dd"),
                mealType.Value,
                ShoppingListUpdated: true
            );
        }

        var mealId = Guid.NewGuid();
        var meal = new PlannedMeal(mealId, request.Date, mealType, request.RecipeId);
        await _mealRepository.AddAsync(meal, cancellationToken);

        _syncService.MarkPendingSync(request.Date);

        return new AddRecipeToMealPlanResultDto(
            mealId,
            meal.Recipe?.Name ?? "Recipe",
            request.Date.ToString("yyyy-MM-dd"),
            mealType.Value,
            ShoppingListUpdated: true
        );
    }
}
