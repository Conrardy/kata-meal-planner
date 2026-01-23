using FluentValidation;

namespace MealPlanner.Application.Recipes.Validators;

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    private static readonly string[] ValidMealTypes = { "Breakfast", "Lunch", "Dinner" };

    public CreateRecipeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Recipe name is required.")
            .MaximumLength(200).WithMessage("Recipe name must not exceed 200 characters.");

        RuleFor(x => x.ImageUrl)
            .Must(BeValidUrlOrNull).WithMessage("Image URL must be a valid URL.")
            .When(x => x.ImageUrl is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Ingredients)
            .NotEmpty().WithMessage("At least one ingredient is required.");

        RuleForEach(x => x.Ingredients)
            .SetValidator(new CreateIngredientDtoValidator());

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("At least one cooking step is required.");

        RuleForEach(x => x.Steps)
            .SetValidator(new CreateCookingStepDtoValidator());

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Cannot have more than 20 tags.");

        RuleFor(x => x.MealType)
            .Must(BeValidMealTypeOrNull).WithMessage($"Meal type must be one of: {string.Join(", ", ValidMealTypes)}.")
            .When(x => x.MealType is not null);
    }

    private static bool BeValidUrlOrNull(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeValidMealTypeOrNull(string? mealType)
    {
        if (string.IsNullOrEmpty(mealType)) return true;
        return ValidMealTypes.Contains(mealType, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class CreateIngredientDtoValidator : AbstractValidator<CreateIngredientDto>
{
    public CreateIngredientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ingredient name is required.")
            .MaximumLength(100).WithMessage("Ingredient name must not exceed 100 characters.");

        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("Ingredient quantity is required.")
            .MaximumLength(50).WithMessage("Ingredient quantity must not exceed 50 characters.");

        RuleFor(x => x.Unit)
            .MaximumLength(30).WithMessage("Ingredient unit must not exceed 30 characters.")
            .When(x => x.Unit is not null);
    }
}

public sealed class CreateCookingStepDtoValidator : AbstractValidator<CreateCookingStepDto>
{
    public CreateCookingStepDtoValidator()
    {
        RuleFor(x => x.StepNumber)
            .GreaterThan(0).WithMessage("Step number must be greater than 0.");

        RuleFor(x => x.Instruction)
            .NotEmpty().WithMessage("Step instruction is required.")
            .MaximumLength(1000).WithMessage("Step instruction must not exceed 1000 characters.");
    }
}
