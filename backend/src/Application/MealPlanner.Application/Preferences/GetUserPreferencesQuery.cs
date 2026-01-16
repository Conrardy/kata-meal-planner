using MediatR;
using MealPlanner.Domain.Preferences;

namespace MealPlanner.Application.Preferences;

public sealed record GetUserPreferencesQuery : IRequest<UserPreferencesDto>;

public sealed class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, UserPreferencesDto>
{
    private readonly IUserPreferencesRepository _repository;

    public GetUserPreferencesQueryHandler(IUserPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserPreferencesDto> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preferences = await _repository.GetAsync(cancellationToken);

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
            MealsPerDay.All.Select(m => m.Value).ToList(),
            PlanLength.All.Select(p => p.Value).ToList()
        );
    }
}
