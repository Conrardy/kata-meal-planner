using FluentValidation;

namespace MealPlanner.Application.DailyDigest.Validators;

public sealed class GetDailyDigestQueryValidator : AbstractValidator<GetDailyDigestQuery>
{
    public GetDailyDigestQueryValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}
