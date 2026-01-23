using FluentAssertions;
using MealPlanner.Domain.Preferences;
using Xunit;

namespace MealPlanner.Domain.Tests.Preferences;

public sealed class UserPreferencesTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldSetDefaultValues()
    {
        // Act
        var preferences = new UserPreferences();

        // Assert
        preferences.DietaryPreference.Should().Be(DietaryPreference.None);
        preferences.Allergies.Should().BeEmpty();
        preferences.MealsPerDay.Should().Be(MealsPerDay.Three);
        preferences.PlanLength.Should().Be(PlanLength.OneWeek);
        preferences.IncludeLeftovers.Should().BeFalse();
        preferences.AutoGenerateShoppingList.Should().BeTrue();
        preferences.ExcludedIngredients.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithAllProperties_ShouldSetAllValues()
    {
        // Arrange
        var allergies = new List<Allergy> { Allergy.Gluten, Allergy.Nuts };
        var excludedIngredients = new List<string> { "cilantro", "olives" };

        // Act
        var preferences = new UserPreferences(
            dietaryPreference: DietaryPreference.Vegetarian,
            allergies: allergies,
            mealsPerDay: MealsPerDay.Four,
            planLength: PlanLength.TwoWeeks,
            includeLeftovers: true,
            autoGenerateShoppingList: false,
            excludedIngredients: excludedIngredients
        );

        // Assert
        preferences.DietaryPreference.Should().Be(DietaryPreference.Vegetarian);
        preferences.Allergies.Should().BeEquivalentTo(allergies);
        preferences.MealsPerDay.Should().Be(MealsPerDay.Four);
        preferences.PlanLength.Should().Be(PlanLength.TwoWeeks);
        preferences.IncludeLeftovers.Should().BeTrue();
        preferences.AutoGenerateShoppingList.Should().BeFalse();
        preferences.ExcludedIngredients.Should().BeEquivalentTo(excludedIngredients);
    }

    [Fact]
    public void Update_ShouldChangeDietaryPreferenceAndAllergies()
    {
        // Arrange
        var preferences = new UserPreferences();
        var newAllergies = new List<Allergy> { Allergy.Dairy, Allergy.Shellfish };

        // Act
        preferences.Update(DietaryPreference.Vegan, newAllergies);

        // Assert
        preferences.DietaryPreference.Should().Be(DietaryPreference.Vegan);
        preferences.Allergies.Should().BeEquivalentTo(newAllergies);
    }

    [Fact]
    public void Update_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var preferences = new UserPreferences(
            mealsPerDay: MealsPerDay.Four,
            planLength: PlanLength.TwoWeeks
        );

        // Act
        preferences.Update(DietaryPreference.Keto, []);

        // Assert
        preferences.MealsPerDay.Should().Be(MealsPerDay.Four);
        preferences.PlanLength.Should().Be(PlanLength.TwoWeeks);
    }

    [Fact]
    public void UpdateMealPlanOptions_ShouldUpdateAllMealPlanSettings()
    {
        // Arrange
        var preferences = new UserPreferences();
        var excludedIngredients = new List<string> { "mushrooms", "onions" };

        // Act
        preferences.UpdateMealPlanOptions(
            MealsPerDay.Two,
            PlanLength.TwoWeeks,
            includeLeftovers: true,
            autoGenerateShoppingList: false,
            excludedIngredients
        );

        // Assert
        preferences.MealsPerDay.Should().Be(MealsPerDay.Two);
        preferences.PlanLength.Should().Be(PlanLength.TwoWeeks);
        preferences.IncludeLeftovers.Should().BeTrue();
        preferences.AutoGenerateShoppingList.Should().BeFalse();
        preferences.ExcludedIngredients.Should().BeEquivalentTo(excludedIngredients);
    }

    [Fact]
    public void UpdateMealPlanOptions_ShouldNotAffectDietarySettings()
    {
        // Arrange
        var allergies = new List<Allergy> { Allergy.Gluten };
        var preferences = new UserPreferences(
            dietaryPreference: DietaryPreference.Mediterranean,
            allergies: allergies
        );

        // Act
        preferences.UpdateMealPlanOptions(
            MealsPerDay.Three,
            PlanLength.OneWeek,
            false,
            true,
            []
        );

        // Assert
        preferences.DietaryPreference.Should().Be(DietaryPreference.Mediterranean);
        preferences.Allergies.Should().BeEquivalentTo(allergies);
    }
}

public sealed class DietaryPreferenceTests
{
    [Fact]
    public void StaticInstances_ShouldHaveCorrectValues()
    {
        DietaryPreference.None.Value.Should().Be("None");
        DietaryPreference.Vegetarian.Value.Should().Be("Vegetarian");
        DietaryPreference.Vegan.Value.Should().Be("Vegan");
        DietaryPreference.Pescatarian.Value.Should().Be("Pescatarian");
        DietaryPreference.Keto.Value.Should().Be("Keto");
        DietaryPreference.Paleo.Value.Should().Be("Paleo");
        DietaryPreference.LowCarb.Value.Should().Be("Low Carb");
        DietaryPreference.Mediterranean.Value.Should().Be("Mediterranean");
    }

    [Theory]
    [InlineData("none", "None")]
    [InlineData("vegetarian", "Vegetarian")]
    [InlineData("vegan", "Vegan")]
    [InlineData("pescatarian", "Pescatarian")]
    [InlineData("keto", "Keto")]
    [InlineData("paleo", "Paleo")]
    [InlineData("low carb", "Low Carb")]
    [InlineData("lowcarb", "Low Carb")]
    [InlineData("mediterranean", "Mediterranean")]
    public void FromString_WithValidValue_ShouldReturnCorrectPreference(string input, string expected)
    {
        // Act
        var preference = DietaryPreference.FromString(input);

        // Assert
        preference.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("None")]
    [InlineData("VEGETARIAN")]
    [InlineData("Vegan")]
    public void FromString_CaseInsensitive_ShouldWork(string input)
    {
        // Act
        var action = () => DietaryPreference.FromString(input);

        // Assert
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("carnivore")]
    [InlineData("raw")]
    [InlineData("")]
    public void FromString_WithInvalidValue_ShouldThrowArgumentException(string input)
    {
        // Act
        var action = () => DietaryPreference.FromString(input);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid dietary preference: {input}*");
    }

    [Fact]
    public void All_ShouldContainAllPreferences()
    {
        // Assert
        DietaryPreference.All.Should().HaveCount(8);
        DietaryPreference.All.Should().Contain(DietaryPreference.None);
        DietaryPreference.All.Should().Contain(DietaryPreference.Vegetarian);
        DietaryPreference.All.Should().Contain(DietaryPreference.Vegan);
        DietaryPreference.All.Should().Contain(DietaryPreference.Pescatarian);
        DietaryPreference.All.Should().Contain(DietaryPreference.Keto);
        DietaryPreference.All.Should().Contain(DietaryPreference.Paleo);
        DietaryPreference.All.Should().Contain(DietaryPreference.LowCarb);
        DietaryPreference.All.Should().Contain(DietaryPreference.Mediterranean);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        DietaryPreference.Vegan.ToString().Should().Be("Vegan");
    }
}

public sealed class AllergyTests
{
    [Fact]
    public void StaticInstances_ShouldHaveCorrectValues()
    {
        Allergy.Gluten.Value.Should().Be("Gluten");
        Allergy.Nuts.Value.Should().Be("Nuts");
        Allergy.Dairy.Value.Should().Be("Dairy");
        Allergy.Eggs.Value.Should().Be("Eggs");
        Allergy.Shellfish.Value.Should().Be("Shellfish");
        Allergy.Soy.Value.Should().Be("Soy");
    }

    [Theory]
    [InlineData("gluten", "Gluten")]
    [InlineData("nuts", "Nuts")]
    [InlineData("dairy", "Dairy")]
    [InlineData("eggs", "Eggs")]
    [InlineData("shellfish", "Shellfish")]
    [InlineData("soy", "Soy")]
    public void FromString_WithValidValue_ShouldReturnCorrectAllergy(string input, string expected)
    {
        // Act
        var allergy = Allergy.FromString(input);

        // Assert
        allergy.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("Gluten")]
    [InlineData("NUTS")]
    [InlineData("Dairy")]
    public void FromString_CaseInsensitive_ShouldWork(string input)
    {
        // Act
        var action = () => Allergy.FromString(input);

        // Assert
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("peanuts")]
    [InlineData("fish")]
    [InlineData("")]
    public void FromString_WithInvalidValue_ShouldThrowArgumentException(string input)
    {
        // Act
        var action = () => Allergy.FromString(input);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid allergy: {input}*");
    }

    [Fact]
    public void All_ShouldContainAllAllergies()
    {
        // Assert
        Allergy.All.Should().HaveCount(6);
        Allergy.All.Should().Contain(Allergy.Gluten);
        Allergy.All.Should().Contain(Allergy.Nuts);
        Allergy.All.Should().Contain(Allergy.Dairy);
        Allergy.All.Should().Contain(Allergy.Eggs);
        Allergy.All.Should().Contain(Allergy.Shellfish);
        Allergy.All.Should().Contain(Allergy.Soy);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        Allergy.Gluten.ToString().Should().Be("Gluten");
    }
}

public sealed class MealsPerDayTests
{
    [Fact]
    public void StaticInstances_ShouldHaveCorrectValues()
    {
        MealsPerDay.Two.Value.Should().Be(2);
        MealsPerDay.Three.Value.Should().Be(3);
        MealsPerDay.Four.Value.Should().Be(4);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void FromInt_WithValidValue_ShouldReturnCorrectMealsPerDay(int input)
    {
        // Act
        var mealsPerDay = MealsPerDay.FromInt(input);

        // Assert
        mealsPerDay.Value.Should().Be(input);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(-1)]
    public void FromInt_WithInvalidValue_ShouldThrowArgumentException(int input)
    {
        // Act
        var action = () => MealsPerDay.FromInt(input);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid meals per day: {input}. Must be 2, 3, or 4.*");
    }

    [Fact]
    public void All_ShouldContainAllOptions()
    {
        // Assert
        MealsPerDay.All.Should().HaveCount(3);
        MealsPerDay.All.Should().Contain(MealsPerDay.Two);
        MealsPerDay.All.Should().Contain(MealsPerDay.Three);
        MealsPerDay.All.Should().Contain(MealsPerDay.Four);
    }

    [Fact]
    public void ToString_ShouldReturnValueAsString()
    {
        MealsPerDay.Two.ToString().Should().Be("2");
        MealsPerDay.Three.ToString().Should().Be("3");
        MealsPerDay.Four.ToString().Should().Be("4");
    }
}

public sealed class PlanLengthTests
{
    [Fact]
    public void StaticInstances_ShouldHaveCorrectValues()
    {
        PlanLength.OneWeek.Value.Should().Be(1);
        PlanLength.TwoWeeks.Value.Should().Be(2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void FromInt_WithValidValue_ShouldReturnCorrectPlanLength(int input)
    {
        // Act
        var planLength = PlanLength.FromInt(input);

        // Assert
        planLength.Value.Should().Be(input);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void FromInt_WithInvalidValue_ShouldThrowArgumentException(int input)
    {
        // Act
        var action = () => PlanLength.FromInt(input);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Invalid plan length: {input}. Must be 1 or 2 weeks.*");
    }

    [Fact]
    public void All_ShouldContainAllOptions()
    {
        // Assert
        PlanLength.All.Should().HaveCount(2);
        PlanLength.All.Should().Contain(PlanLength.OneWeek);
        PlanLength.All.Should().Contain(PlanLength.TwoWeeks);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        PlanLength.OneWeek.ToString().Should().Be("1 week");
        PlanLength.TwoWeeks.ToString().Should().Be("2 weeks");
    }
}
