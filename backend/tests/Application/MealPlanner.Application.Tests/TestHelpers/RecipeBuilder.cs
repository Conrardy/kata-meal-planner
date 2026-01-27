using MealPlanner.Domain.Meals;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Application.Tests.TestHelpers;

public sealed class RecipeBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Recipe";
    private string? _imageUrl;
    private string? _description;
    private List<string> _tags = [];
    private MealType _mealType = MealType.Dinner;
    private List<Ingredient> _ingredients = [new("Test Ingredient", "1", "unit")];
    private List<CookingStep> _steps = [new(1, "Test step")];

    public static RecipeBuilder Create() => new();

    public RecipeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RecipeBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public RecipeBuilder WithImageUrl(string? imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public RecipeBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public RecipeBuilder WithTags(params string[] tags)
    {
        _tags = [.. tags];
        return this;
    }

    public RecipeBuilder WithMealType(MealType mealType)
    {
        _mealType = mealType;
        return this;
    }

    public RecipeBuilder WithIngredients(params Ingredient[] ingredients)
    {
        _ingredients = [.. ingredients];
        return this;
    }

    public RecipeBuilder WithSteps(params CookingStep[] steps)
    {
        _steps = [.. steps];
        return this;
    }

    public Recipe Build()
    {
        return new Recipe(
            _id,
            _name,
            _imageUrl,
            _description,
            _tags,
            _mealType,
            _ingredients,
            _steps
        );
    }
}
