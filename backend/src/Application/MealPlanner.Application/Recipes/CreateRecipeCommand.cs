using MediatR;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Recipes;

public sealed record CreateIngredientDto(string Name, string Quantity, string? Unit);
public sealed record CreateCookingStepDto(int StepNumber, string Instruction);

public sealed record CreateRecipeCommand(
    string Name,
    string? ImageUrl,
    string? Description,
    IReadOnlyList<CreateIngredientDto> Ingredients,
    IReadOnlyList<CreateCookingStepDto> Steps,
    IReadOnlyList<string>? Tags,
    string? MealType
) : IRequest<Guid>;

public sealed class CreateRecipeCommandHandler : IRequestHandler<CreateRecipeCommand, Guid>
{
    private readonly IRecipeRepository _repository;

    public CreateRecipeCommandHandler(IRecipeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        var ingredients = request.Ingredients?
            .Select(i => new Ingredient(i.Name, i.Quantity, i.Unit))
            .ToList() ?? new List<Ingredient>();

        var steps = request.Steps?
            .Select(s => new CookingStep(s.StepNumber, s.Instruction))
            .ToList() ?? new List<CookingStep>();

        MealType? mealType = null;
        if (!string.IsNullOrWhiteSpace(request.MealType))
        {
            mealType = MealType.FromString(request.MealType!);
        }

        var recipe = new Recipe(
            id,
            request.Name,
            request.ImageUrl,
            request.Description,
            request.Tags,
            mealType,
            ingredients,
            steps
        );

        await _repository.AddAsync(recipe, cancellationToken);
        return id;
    }
}
