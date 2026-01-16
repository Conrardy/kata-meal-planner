using MediatR;
using MealPlanner.Domain.Meals;

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
    string MealType
);

public sealed class AddRecipeToMealPlanCommandHandler : IRequestHandler<AddRecipeToMealPlanCommand, AddRecipeToMealPlanResultDto>
{
    private readonly IPlannedMealRepository _mealRepository;

    public AddRecipeToMealPlanCommandHandler(IPlannedMealRepository mealRepository)
    {
        _mealRepository = mealRepository;
    }

    public async Task<AddRecipeToMealPlanResultDto> Handle(AddRecipeToMealPlanCommand request, CancellationToken cancellationToken)
    {
        var mealType = MealType.FromString(request.MealType);
        var mealId = Guid.NewGuid();

        var meal = new PlannedMeal(mealId, request.Date, mealType, request.RecipeId);
        await _mealRepository.AddAsync(meal, cancellationToken);

        return new AddRecipeToMealPlanResultDto(
            mealId,
            meal.Recipe?.Name ?? "Recipe",
            request.Date.ToString("yyyy-MM-dd"),
            mealType.Value
        );
    }
}
