using FluentValidation;

namespace MealPlanner.Application.WeeklyPlan.Validators;

public sealed class GetWeeklyPlanQueryValidator : AbstractValidator<GetWeeklyPlanQuery>
{
    public GetWeeklyPlanQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");
    }
}
