using FluentValidation;

namespace MealPlanner.Application.ShoppingList.Validators;

public sealed class AddCustomItemCommandValidator : AbstractValidator<AddCustomItemCommand>
{
    private static readonly string[] ValidCategories = { "Produce", "Dairy", "Meat", "Pantry" };

    public AddCustomItemCommandValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(100).WithMessage("Item name must not exceed 100 characters.");

        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("Quantity is required.")
            .MaximumLength(50).WithMessage("Quantity must not exceed 50 characters.");

        RuleFor(x => x.Unit)
            .MaximumLength(30).WithMessage("Unit must not exceed 30 characters.")
            .When(x => x.Unit is not null);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .Must(BeValidCategory).WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}.");
    }

    private static bool BeValidCategory(string category)
    {
        return ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }
}
