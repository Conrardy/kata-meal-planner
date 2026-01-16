using MediatR;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Recipes;

public sealed record GetRecipeDetailsQuery(Guid RecipeId) : IRequest<RecipeDetailsDto?>;

public sealed record IngredientDto(string Name, string Quantity, string? Unit);
public sealed record CookingStepDto(int StepNumber, string Instruction);

public sealed record RecipeDetailsDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    string? Description,
    IReadOnlyList<string> Tags,
    string MealType,
    IReadOnlyList<IngredientDto> Ingredients,
    IReadOnlyList<CookingStepDto> Steps
);

public sealed class GetRecipeDetailsQueryHandler : IRequestHandler<GetRecipeDetailsQuery, RecipeDetailsDto?>
{
    private readonly IRecipeRepository _repository;

    public GetRecipeDetailsQueryHandler(IRecipeRepository repository)
    {
        _repository = repository;
    }

    public async Task<RecipeDetailsDto?> Handle(GetRecipeDetailsQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _repository.GetByIdAsync(request.RecipeId, cancellationToken);

        if (recipe is null)
            return null;

        return new RecipeDetailsDto(
            recipe.Id,
            recipe.Name,
            recipe.ImageUrl,
            recipe.Description,
            recipe.Tags,
            recipe.MealType.Value,
            recipe.Ingredients.Select(i => new IngredientDto(i.Name, i.Quantity, i.Unit)).ToList(),
            recipe.Steps.Select(s => new CookingStepDto(s.StepNumber, s.Instruction)).ToList()
        );
    }
}
