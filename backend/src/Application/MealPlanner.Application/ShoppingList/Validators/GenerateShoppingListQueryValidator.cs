using FluentValidation;

namespace MealPlanner.Application.ShoppingList.Validators;

public sealed class GenerateShoppingListQueryValidator : AbstractValidator<GenerateShoppingListQuery>
{
    public GenerateShoppingListQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");
    }
}
