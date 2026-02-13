using FluentValidation;

namespace MealPlanner.Application.Stock.Validators;

public sealed class AdjustStockQuantityCommandValidator : AbstractValidator<AdjustStockQuantityCommand>
{
    public AdjustStockQuantityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stock item ID is required.");

        RuleFor(x => x.Adjustment)
            .NotEqual(0).WithMessage("Adjustment must be non-zero.");
    }
}
