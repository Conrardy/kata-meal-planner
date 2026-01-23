using FluentValidation;

namespace MealPlanner.Application.ShoppingList.Validators;

public sealed class ToggleShoppingItemCommandValidator : AbstractValidator<ToggleShoppingItemCommand>
{
    public ToggleShoppingItemCommandValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");
    }
}
