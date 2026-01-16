namespace MealPlanner.Domain.Recipes;

public sealed class Recipe
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyList<string> Tags { get; private set; }

    private Recipe() { Name = string.Empty; Tags = []; }

    public Recipe(Guid id, string name, string? imageUrl = null, string? description = null, IReadOnlyList<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Recipe name cannot be empty", nameof(name));

        Id = id;
        Name = name;
        ImageUrl = imageUrl;
        Description = description;
        Tags = tags ?? [];
    }
}
