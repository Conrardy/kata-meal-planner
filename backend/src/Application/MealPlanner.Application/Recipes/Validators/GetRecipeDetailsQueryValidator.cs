using FluentValidation;

namespace MealPlanner.Application.Recipes.Validators;

public sealed class GetRecipeDetailsQueryValidator : AbstractValidator<GetRecipeDetailsQuery>
{
    public GetRecipeDetailsQueryValidator()
    {
        RuleFor(x => x.RecipeId)
            .NotEmpty().WithMessage("Recipe ID is required.");
    }
}
