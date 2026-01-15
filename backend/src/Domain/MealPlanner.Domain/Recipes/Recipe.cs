namespace MealPlanner.Domain.Recipes;

public sealed class Recipe
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Description { get; private set; }

    private Recipe() { Name = string.Empty; }

    public Recipe(Guid id, string name, string? imageUrl = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Recipe name cannot be empty", nameof(name));

        Id = id;
        Name = name;
        ImageUrl = imageUrl;
        Description = description;
    }
}
