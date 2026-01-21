public record CreateRecipeRequest(
    string Name,
    string? ImageUrl,
    string? Description,
    IReadOnlyList<CreateIngredientRequest> Ingredients,
    IReadOnlyList<CreateCookingStepRequest> Steps,
    IReadOnlyList<string>? Tags,
    string? MealType
);

public record CreateIngredientRequest(string Name, string Quantity, string? Unit);
public record CreateCookingStepRequest(int StepNumber, string Instruction);
