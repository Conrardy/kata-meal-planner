using MediatR;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Recipes;

public sealed record SearchRecipesQuery(string? SearchTerm, IReadOnlyList<string>? Tags) : IRequest<RecipeSearchResultDto>;

public sealed record RecipeSearchResultDto(
    IReadOnlyList<RecipeItemDto> Recipes,
    IReadOnlyList<string> AvailableTags
);

public sealed record RecipeItemDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    string? Description,
    IReadOnlyList<string> Tags
);

public sealed class SearchRecipesQueryHandler : IRequestHandler<SearchRecipesQuery, RecipeSearchResultDto>
{
    private readonly IRecipeRepository _repository;

    public SearchRecipesQueryHandler(IRecipeRepository repository)
    {
        _repository = repository;
    }

    public async Task<RecipeSearchResultDto> Handle(SearchRecipesQuery request, CancellationToken cancellationToken)
    {
        var recipes = await _repository.SearchAsync(request.SearchTerm, request.Tags, cancellationToken);
        var allTags = await _repository.GetAllTagsAsync(cancellationToken);

        var recipeDtos = recipes.Select(r => new RecipeItemDto(
            r.Id,
            r.Name,
            r.ImageUrl,
            r.Description,
            r.Tags
        )).ToList();

        return new RecipeSearchResultDto(recipeDtos, allTags);
    }
}
