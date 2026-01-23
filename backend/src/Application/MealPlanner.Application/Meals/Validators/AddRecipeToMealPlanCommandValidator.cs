using FluentValidation;
using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals.Validators;

public sealed class AddRecipeToMealPlanCommandValidator : AbstractValidator<AddRecipeToMealPlanCommand>
{
    private static readonly string[] ValidMealTypes = { "Breakfast", "Lunch", "Dinner" };

    public AddRecipeToMealPlanCommandValidator()
    {
        RuleFor(x => x.RecipeId)
            .NotEmpty().WithMessage("Recipe ID is required.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.MealType)
            .NotEmpty().WithMessage("Meal type is required.")
            .Must(BeValidMealType).WithMessage($"Meal type must be one of: {string.Join(", ", ValidMealTypes)}.");
    }

    private static bool BeValidMealType(string mealType)
    {
        return ValidMealTypes.Contains(mealType, StringComparer.OrdinalIgnoreCase);
    }
}
