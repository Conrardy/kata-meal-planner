using FluentValidation;
using MealPlanner.Domain.Preferences;

namespace MealPlanner.Application.Preferences.Validators;

public sealed class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
{
    private static readonly string[] ValidDietaryPreferences =
        DietaryPreference.All.Select(d => d.Value).ToArray();

    private static readonly string[] ValidAllergies =
        Allergy.All.Select(a => a.Value).ToArray();

    private static readonly int[] ValidMealsPerDay =
        MealsPerDay.All.Select(m => m.Value).ToArray();

    private static readonly int[] ValidPlanLengths =
        PlanLength.All.Select(p => p.Value).ToArray();

    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.DietaryPreference)
            .NotEmpty().WithMessage("Dietary preference is required.")
            .Must(BeValidDietaryPreference)
            .WithMessage($"Dietary preference must be one of: {string.Join(", ", ValidDietaryPreferences)}.");

        RuleFor(x => x.Allergies)
            .NotNull().WithMessage("Allergies list is required.");

        RuleForEach(x => x.Allergies)
            .Must(BeValidAllergy)
            .WithMessage($"Allergy must be one of: {string.Join(", ", ValidAllergies)}.");

        RuleFor(x => x.MealsPerDay)
            .Must(BeValidMealsPerDay)
            .WithMessage($"Meals per day must be one of: {string.Join(", ", ValidMealsPerDay)}.")
            .When(x => x.MealsPerDay.HasValue);

        RuleFor(x => x.PlanLength)
            .Must(BeValidPlanLength)
            .WithMessage($"Plan length must be one of: {string.Join(", ", ValidPlanLengths)}.")
            .When(x => x.PlanLength.HasValue);

        RuleFor(x => x.ExcludedIngredients)
            .Must(ingredients => ingredients == null || ingredients.Count <= 50)
            .WithMessage("Cannot exclude more than 50 ingredients.");

        RuleForEach(x => x.ExcludedIngredients)
            .NotEmpty().WithMessage("Excluded ingredient cannot be empty.")
            .MaximumLength(100).WithMessage("Excluded ingredient must not exceed 100 characters.")
            .When(x => x.ExcludedIngredients is not null);
    }

    private static bool BeValidDietaryPreference(string preference)
    {
        return ValidDietaryPreferences.Contains(preference, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidAllergy(string allergy)
    {
        return ValidAllergies.Contains(allergy, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidMealsPerDay(int? mealsPerDay)
    {
        return mealsPerDay.HasValue && ValidMealsPerDay.Contains(mealsPerDay.Value);
    }

    private static bool BeValidPlanLength(int? planLength)
    {
        return planLength.HasValue && ValidPlanLengths.Contains(planLength.Value);
    }
}
