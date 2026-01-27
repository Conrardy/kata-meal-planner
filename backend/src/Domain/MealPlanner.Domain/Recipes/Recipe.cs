using MealPlanner.Domain.Meals;

namespace MealPlanner.Domain.Recipes;

public sealed record Ingredient(string Name, string Quantity, string? Unit = null);

public sealed record CookingStep(int StepNumber, string Instruction);

public sealed class Recipe
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyList<string> Tags { get; private set; }
    public MealType MealType { get; private set; }
    public IReadOnlyList<Ingredient> Ingredients { get; private set; }
    public IReadOnlyList<CookingStep> Steps { get; private set; }

    private Recipe() { Name = string.Empty; Tags = []; MealType = MealType.Dinner; Ingredients = []; Steps = []; }

    public Recipe(
        Guid id,
        string name,
        string? imageUrl = null,
        string? description = null,
        IReadOnlyList<string>? tags = null,
        MealType? mealType = null,
        IReadOnlyList<Ingredient>? ingredients = null,
        IReadOnlyList<CookingStep>? steps = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Recipe name cannot be empty", nameof(name));

        if (ingredients == null || ingredients.Count == 0)
            throw new ArgumentException("Recipe must have at least one ingredient", nameof(ingredients));

        if (steps == null || steps.Count == 0)
            throw new ArgumentException("Recipe must have at least one step", nameof(steps));

        Id = id;
        Name = name;
        ImageUrl = imageUrl;
        Description = description;
        Tags = tags ?? [];
        MealType = mealType ?? MealType.Dinner;
        Ingredients = ingredients;
        Steps = steps;
    }
}
