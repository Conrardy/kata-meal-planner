using FluentAssertions;
using MealPlanner.Application.Preferences;
using MealPlanner.Domain.Preferences;
using Xunit;

namespace MealPlanner.Application.Tests.Preferences.Handlers;

public sealed class GetUserPreferencesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUserPreferences()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new GetUserPreferencesQueryHandler(repository);
        var query = new GetUserPreferencesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDefaultPreferences_ShouldReturnDefaultValues()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new GetUserPreferencesQueryHandler(repository);
        var query = new GetUserPreferencesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.DietaryPreference.Should().Be("None");
        result.Allergies.Should().BeEmpty();
        result.MealsPerDay.Should().Be(3);
        result.PlanLength.Should().Be(1);
        result.IncludeLeftovers.Should().BeFalse();
        result.AutoGenerateShoppingList.Should().BeTrue();
        result.ExcludedIngredients.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCustomPreferences_ShouldReturnCustomValues()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var preferences = new UserPreferences(
            dietaryPreference: DietaryPreference.Vegan,
            allergies: [Allergy.Gluten, Allergy.Nuts],
            mealsPerDay: MealsPerDay.Four,
            planLength: PlanLength.TwoWeeks,
            includeLeftovers: true,
            autoGenerateShoppingList: false,
            excludedIngredients: ["mushrooms"]
        );
        repository.SetPreferences(preferences);

        var handler = new GetUserPreferencesQueryHandler(repository);
        var query = new GetUserPreferencesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.DietaryPreference.Should().Be("Vegan");
        result.Allergies.Should().BeEquivalentTo(["Gluten", "Nuts"]);
        result.MealsPerDay.Should().Be(4);
        result.PlanLength.Should().Be(2);
        result.IncludeLeftovers.Should().BeTrue();
        result.AutoGenerateShoppingList.Should().BeFalse();
        result.ExcludedIngredients.Should().BeEquivalentTo(["mushrooms"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllAvailableDietaryPreferences()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new GetUserPreferencesQueryHandler(repository);
        var query = new GetUserPreferencesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.AvailableDietaryPreferences.Should().HaveCount(8);
        result.AvailableDietaryPreferences.Should().Contain("None");
        result.AvailableDietaryPreferences.Should().Contain("Vegetarian");
        result.AvailableDietaryPreferences.Should().Contain("Vegan");
        result.AvailableDietaryPreferences.Should().Contain("Keto");
        result.AvailableDietaryPreferences.Should().Contain("Mediterranean");
    }

    [Fact]
    public async Task Handle_ShouldReturnAllAvailableAllergies()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new GetUserPreferencesQueryHandler(repository);
        var query = new GetUserPreferencesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.AvailableAllergies.Should().HaveCount(6);
        result.AvailableAllergies.Should().Contain("Gluten");
        result.AvailableAllergies.Should().Contain("Nuts");
        result.AvailableAllergies.Should().Contain("Dairy");
        result.AvailableAllergies.Should().Contain("Eggs");
        result.AvailableAllergies.Should().Contain("Shellfish");
        result.AvailableAllergies.Should().Contain("Soy");
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailableMealsPerDayOptions()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new GetUserPreferencesQueryHandler(repository);
        var query = new GetUserPreferencesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.AvailableMealsPerDay.Should().BeEquivalentTo([2, 3, 4]);
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailablePlanLengthOptions()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new GetUserPreferencesQueryHandler(repository);
        var query = new GetUserPreferencesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.AvailablePlanLengths.Should().BeEquivalentTo([1, 2]);
    }

    private sealed class InMemoryUserPreferencesRepository : IUserPreferencesRepository
    {
        private UserPreferences _preferences = new();

        public void SetPreferences(UserPreferences preferences) => _preferences = preferences;

        public Task<UserPreferences> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_preferences);

        public Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
        {
            _preferences = preferences;
            return Task.CompletedTask;
        }
    }
}
