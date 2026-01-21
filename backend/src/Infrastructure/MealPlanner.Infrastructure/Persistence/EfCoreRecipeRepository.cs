using MealPlanner.Domain.Recipes;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Persistence;

public sealed class EfCoreRecipeRepository : IRecipeRepository
{
    private readonly MealPlannerDbContext _context;

    public EfCoreRecipeRepository(MealPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Recipe>> GetSuggestionsAsync(Guid excludeRecipeId, CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .Where(r => r.Id != excludeRecipeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Recipe>> SearchAsync(string? searchTerm, IReadOnlyList<string>? tags, CancellationToken cancellationToken = default)
    {
        var query = _context.Recipes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(r =>
                EF.Functions.ILike(r.Name, $"%{term}%") ||
                (r.Description != null && EF.Functions.ILike(r.Description, $"%{term}%")));
        }

        if (tags is { Count: > 0 })
        {
            query = query.Where(r => r.Tags.Any(t => tags.Contains(t)));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        var recipes = await _context.Recipes
            .AsNoTracking()
            .Select(r => r.Tags)
            .ToListAsync(cancellationToken);

        return recipes
            .SelectMany(t => t)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    public async Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        await _context.Recipes.AddAsync(recipe, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
