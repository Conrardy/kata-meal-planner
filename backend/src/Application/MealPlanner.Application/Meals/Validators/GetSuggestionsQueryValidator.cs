using FluentValidation;

namespace MealPlanner.Application.Meals.Validators;

public sealed class GetSuggestionsQueryValidator : AbstractValidator<GetSuggestionsQuery>
{
    public GetSuggestionsQueryValidator()
    {
        RuleFor(x => x.MealId)
            .NotEmpty().WithMessage("Meal ID is required.");
    }
}
