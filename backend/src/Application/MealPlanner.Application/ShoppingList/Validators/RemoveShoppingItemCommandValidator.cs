using FluentValidation;

namespace MealPlanner.Application.ShoppingList.Validators;

public sealed class RemoveShoppingItemCommandValidator : AbstractValidator<RemoveShoppingItemCommand>
{
    public RemoveShoppingItemCommandValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");
    }
}
