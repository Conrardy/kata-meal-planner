using MediatR;
using MealPlanner.Domain.Preferences;

namespace MealPlanner.Application.Preferences;

public sealed record UpdateUserPreferencesCommand(
    string DietaryPreference,
    IReadOnlyList<string> Allergies,
    int? MealsPerDay = null,
    int? PlanLength = null,
    bool? IncludeLeftovers = null,
    bool? AutoGenerateShoppingList = null,
    IReadOnlyList<string>? ExcludedIngredients = null
) : IRequest<UserPreferencesDto>;

public sealed class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand, UserPreferencesDto>
{
    private readonly IUserPreferencesRepository _repository;

    public UpdateUserPreferencesCommandHandler(IUserPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserPreferencesDto> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var dietaryPreference = DietaryPreference.FromString(request.DietaryPreference);
        var allergies = request.Allergies.Select(Allergy.FromString).ToList();

        var preferences = await _repository.GetAsync(cancellationToken);
        preferences.Update(dietaryPreference, allergies);

        if (request.MealsPerDay.HasValue ||
            request.PlanLength.HasValue ||
            request.IncludeLeftovers.HasValue ||
            request.AutoGenerateShoppingList.HasValue ||
            request.ExcludedIngredients != null)
        {
            var mealsPerDay = request.MealsPerDay.HasValue
                ? Domain.Preferences.MealsPerDay.FromInt(request.MealsPerDay.Value)
                : preferences.MealsPerDay;
            var planLength = request.PlanLength.HasValue
                ? Domain.Preferences.PlanLength.FromInt(request.PlanLength.Value)
                : preferences.PlanLength;
            var includeLeftovers = request.IncludeLeftovers ?? preferences.IncludeLeftovers;
            var autoGenerateShoppingList = request.AutoGenerateShoppingList ?? preferences.AutoGenerateShoppingList;
            var excludedIngredients = request.ExcludedIngredients ?? preferences.ExcludedIngredients;

            preferences.UpdateMealPlanOptions(mealsPerDay, planLength, includeLeftovers, autoGenerateShoppingList, excludedIngredients);
        }

        await _repository.SaveAsync(preferences, cancellationToken);

        return new UserPreferencesDto(
            preferences.DietaryPreference.Value,
            preferences.Allergies.Select(a => a.Value).ToList(),
            DietaryPreference.All.Select(d => d.Value).ToList(),
            Allergy.All.Select(a => a.Value).ToList(),
            preferences.MealsPerDay.Value,
            preferences.PlanLength.Value,
            preferences.IncludeLeftovers,
            preferences.AutoGenerateShoppingList,
            preferences.ExcludedIngredients.ToList(),
            Domain.Preferences.MealsPerDay.All.Select(m => m.Value).ToList(),
            Domain.Preferences.PlanLength.All.Select(p => p.Value).ToList()
        );
    }
}
