using MediatR;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Recipes;

public sealed record GetRecipeDetailsQuery(Guid RecipeId) : IRequest<RecipeDetailsDto?>;

public sealed record RecipeDetailsDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    string? Description
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
            recipe.Description
        );
    }
}
