using FluentAssertions;
using MealPlanner.Application.Preferences;
using MealPlanner.Domain.Preferences;
using Xunit;

namespace MealPlanner.Application.Tests.Preferences.Handlers;

public sealed class UpdateUserPreferencesCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateDietaryPreference()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "Vegetarian",
            Allergies: []
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DietaryPreference.Should().Be("Vegetarian");
    }

    [Fact]
    public async Task Handle_WithAllergies_ShouldUpdateAllergies()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: ["Gluten", "Nuts"]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Allergies.Should().BeEquivalentTo(["Gluten", "Nuts"]);
    }

    [Fact]
    public async Task Handle_WithMealPlanOptions_ShouldUpdateOptions()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: [],
            MealsPerDay: 4,
            PlanLength: 2,
            IncludeLeftovers: true,
            AutoGenerateShoppingList: false,
            ExcludedIngredients: ["cilantro", "olives"]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.MealsPerDay.Should().Be(4);
        result.PlanLength.Should().Be(2);
        result.IncludeLeftovers.Should().BeTrue();
        result.AutoGenerateShoppingList.Should().BeFalse();
        result.ExcludedIngredients.Should().BeEquivalentTo(["cilantro", "olives"]);
    }

    [Fact]
    public async Task Handle_WithPartialMealPlanOptions_ShouldPreserveExisting()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        repository.SetPreferences(new UserPreferences(
            mealsPerDay: MealsPerDay.Four,
            planLength: PlanLength.TwoWeeks,
            includeLeftovers: true
        ));

        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "Keto",
            Allergies: [],
            MealsPerDay: 2
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.MealsPerDay.Should().Be(2);
        result.PlanLength.Should().Be(2);
        result.IncludeLeftovers.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllAvailableOptions()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: []
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.AvailableDietaryPreferences.Should().Contain("None");
        result.AvailableDietaryPreferences.Should().Contain("Vegetarian");
        result.AvailableDietaryPreferences.Should().Contain("Vegan");
        result.AvailableDietaryPreferences.Should().HaveCount(8);

        result.AvailableAllergies.Should().Contain("Gluten");
        result.AvailableAllergies.Should().Contain("Nuts");
        result.AvailableAllergies.Should().HaveCount(6);

        result.AvailableMealsPerDay.Should().BeEquivalentTo([2, 3, 4]);
        result.AvailablePlanLengths.Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task Handle_WithInvalidDietaryPreference_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "InvalidPreference",
            Allergies: []
        );

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid dietary preference*");
    }

    [Fact]
    public async Task Handle_WithInvalidAllergy_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: ["InvalidAllergy"]
        );

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid allergy*");
    }

    [Fact]
    public async Task Handle_WithInvalidMealsPerDay_ShouldThrowArgumentException()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "None",
            Allergies: [],
            MealsPerDay: 5
        );

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid meals per day*");
    }

    [Fact]
    public async Task Handle_ShouldSavePreferencesToRepository()
    {
        // Arrange
        var repository = new InMemoryUserPreferencesRepository();
        var handler = new UpdateUserPreferencesCommandHandler(repository);
        var command = new UpdateUserPreferencesCommand(
            DietaryPreference: "Mediterranean",
            Allergies: ["Dairy"]
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.SavedPreferences.Should().NotBeNull();
        repository.SavedPreferences!.DietaryPreference.Value.Should().Be("Mediterranean");
        repository.SavedPreferences.Allergies.Should().HaveCount(1);
    }

    private sealed class InMemoryUserPreferencesRepository : IUserPreferencesRepository
    {
        private UserPreferences _preferences = new();
        public UserPreferences? SavedPreferences { get; private set; }

        public void SetPreferences(UserPreferences preferences)
        {
            _preferences = preferences;
        }

        public Task<UserPreferences> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_preferences);

        public Task SaveAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
        {
            SavedPreferences = preferences;
            _preferences = preferences;
            return Task.CompletedTask;
        }
    }
}
