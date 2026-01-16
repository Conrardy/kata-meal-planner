using MediatR;
using MealPlanner.Domain.Preferences;

namespace MealPlanner.Application.Preferences;

public sealed record UpdateUserPreferencesCommand(
    string DietaryPreference,
    IReadOnlyList<string> Allergies
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
        await _repository.SaveAsync(preferences, cancellationToken);

        return new UserPreferencesDto(
            preferences.DietaryPreference.Value,
            preferences.Allergies.Select(a => a.Value).ToList(),
            DietaryPreference.All.Select(d => d.Value).ToList(),
            Allergy.All.Select(a => a.Value).ToList()
        );
    }
}
