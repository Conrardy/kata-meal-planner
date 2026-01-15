namespace MealPlanner.Domain.Recipes;

public interface IRecipeRepository
{
    Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetSuggestionsAsync(Guid excludeRecipeId, CancellationToken cancellationToken = default);
}
