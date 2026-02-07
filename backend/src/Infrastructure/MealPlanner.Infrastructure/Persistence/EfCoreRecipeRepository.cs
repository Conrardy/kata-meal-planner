using MealPlanner.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

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
        var conditions = new List<string>();
        var parameters = new List<object>();

        var searchTermParam = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        if (!string.IsNullOrWhiteSpace(searchTermParam))
        {
            conditions.Add("(name ILIKE '%' || @search_term || '%' OR (description IS NOT NULL AND description ILIKE '%' || @search_term || '%'))");
            parameters.Add(new NpgsqlParameter("search_term", searchTermParam));
        }

        var tagsParam = tags is { Count: > 0 }
            ? tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToArray()
            : Array.Empty<string>();

        if (tagsParam.Length > 0)
        {
            conditions.Add("tags && @tags_param");
            parameters.Add(new NpgsqlParameter("tags_param", NpgsqlDbType.Array | NpgsqlDbType.Text)
            {
                Value = tagsParam
            });
        }

        var sql = "SELECT * FROM recipes";
        if (conditions.Count > 0)
        {
            sql += " WHERE 1=1 AND " + string.Join(" AND ", conditions);
        }

        return await _context.Recipes
            .FromSqlRaw(sql, parameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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
