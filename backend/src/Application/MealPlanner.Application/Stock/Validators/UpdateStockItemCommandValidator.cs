using FluentValidation;

namespace MealPlanner.Application.Stock.Validators;

public sealed class UpdateStockItemCommandValidator : AbstractValidator<UpdateStockItemCommand>
{
    private static readonly string[] ValidCategories = ["Produce", "Dairy", "Meat", "Pantry"];

    public UpdateStockItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stock item ID is required.");

        RuleFor(x => x.IngredientName)
            .NotEmpty().WithMessage("Ingredient name is required.")
            .MaximumLength(200).WithMessage("Ingredient name must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .Must(BeValidCategory).WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}.");

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold must be greater than or equal to 0.")
            .When(x => x.LowStockThreshold.HasValue);
    }

    private static bool BeValidCategory(string category)
    {
        return ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }
}
