using MediatR;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Meals;

public sealed record SwapMealCommand(Guid MealId, Guid NewRecipeId) : IRequest<SwapMealResultDto>;

public sealed class SwapMealCommandHandler : IRequestHandler<SwapMealCommand, SwapMealResultDto>
{
    private readonly IPlannedMealRepository _mealRepository;
    private readonly IRecipeRepository _recipeRepository;

    public SwapMealCommandHandler(IPlannedMealRepository mealRepository, IRecipeRepository recipeRepository)
    {
        _mealRepository = mealRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<SwapMealResultDto> Handle(SwapMealCommand request, CancellationToken cancellationToken)
    {
        var meal = await _mealRepository.GetByIdAsync(request.MealId, cancellationToken);
        if (meal == null)
        {
            throw new InvalidOperationException($"Meal with ID {request.MealId} not found");
        }

        var newRecipe = await _recipeRepository.GetByIdAsync(request.NewRecipeId, cancellationToken);
        if (newRecipe == null)
        {
            throw new InvalidOperationException($"Recipe with ID {request.NewRecipeId} not found");
        }

        meal.SwapRecipe(request.NewRecipeId);
        await _mealRepository.UpdateAsync(meal, cancellationToken);

        return new SwapMealResultDto(
            meal.Id,
            meal.MealType.Value,
            newRecipe.Name,
            newRecipe.ImageUrl
        );
    }
}
