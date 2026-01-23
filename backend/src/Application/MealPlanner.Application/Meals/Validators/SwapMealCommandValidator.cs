using FluentValidation;

namespace MealPlanner.Application.Meals.Validators;

public sealed class SwapMealCommandValidator : AbstractValidator<SwapMealCommand>
{
    public SwapMealCommandValidator()
    {
        RuleFor(x => x.MealId)
            .NotEmpty().WithMessage("Meal ID is required.");

        RuleFor(x => x.NewRecipeId)
            .NotEmpty().WithMessage("New recipe ID is required.");
    }
}
