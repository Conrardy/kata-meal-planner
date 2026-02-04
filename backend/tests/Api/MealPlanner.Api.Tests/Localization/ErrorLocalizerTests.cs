using System.Globalization;
using FluentAssertions;
using MealPlanner.Api.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace MealPlanner.Api.Tests.Localization;

public sealed class ErrorLocalizerTests
{
    private static ErrorLocalizer CreateLocalizer(string culture)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        var provider = services.BuildServiceProvider();

        CultureInfo.CurrentUICulture = new CultureInfo(culture);

        var stringLocalizer = provider.GetRequiredService<IStringLocalizer<SharedResource>>();
        return new ErrorLocalizer(stringLocalizer);
    }

    [Fact]
    public void Localize_WithEnglishCulture_ReturnsEnglishMessage()
    {
        var localizer = CreateLocalizer("en");

        var result = localizer.Localize("Admin.UsernameAlreadyExists", "fallback");

        result.Should().Be("A user with this username already exists.");
    }

    [Fact]
    public void Localize_WithFrenchCulture_ReturnsFrenchMessage()
    {
        var localizer = CreateLocalizer("fr");

        var result = localizer.Localize("Admin.UsernameAlreadyExists", "fallback");

        result.Should().Be("Un utilisateur avec ce nom d'utilisateur existe déjà.");
    }

    [Fact]
    public void Localize_WithUnknownCode_ReturnsFallback()
    {
        var localizer = CreateLocalizer("en");

        var result = localizer.Localize("Unknown.Code", "This is the fallback");

        result.Should().Be("This is the fallback");
    }

    [Fact]
    public void Localize_WithUnsupportedCulture_FallsBackToEnglish()
    {
        var localizer = CreateLocalizer("de");

        var result = localizer.Localize("Auth.InvalidCredentials", "fallback");

        result.Should().Be("Invalid username or password.");
    }

    [Theory]
    [InlineData("en", "Auth.InvalidCredentials", "Invalid username or password.")]
    [InlineData("fr", "Auth.InvalidCredentials", "Nom d'utilisateur ou mot de passe invalide.")]
    [InlineData("en", "Auth.EmailAlreadyExists", "A user with this email already exists.")]
    [InlineData("fr", "Auth.EmailAlreadyExists", "Un utilisateur avec cette adresse e-mail existe déjà.")]
    [InlineData("en", "Auth.InvalidRefreshToken", "The refresh token is invalid or expired.")]
    [InlineData("fr", "Auth.InvalidRefreshToken", "Le jeton de rafraîchissement est invalide ou expiré.")]
    [InlineData("en", "Auth.UserNotFound", "User not found.")]
    [InlineData("fr", "Auth.UserNotFound", "Utilisateur non trouvé.")]
    public void Localize_AuthErrors_ReturnsCorrectTranslation(string culture, string code, string expected)
    {
        var localizer = CreateLocalizer(culture);

        var result = localizer.Localize(code, "fallback");

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("en", "Meal.NotFound", "Meal with ID abc was not found.")]
    [InlineData("fr", "Meal.NotFound", "Le repas avec l'identifiant abc n'a pas été trouvé.")]
    [InlineData("en", "Meal.RecipeNotFound", "Recipe with ID abc was not found.")]
    [InlineData("fr", "Meal.RecipeNotFound", "La recette avec l'identifiant abc n'a pas été trouvée.")]
    public void Localize_MealErrorsWithArgs_ReturnsFormattedTranslation(string culture, string code, string expected)
    {
        var localizer = CreateLocalizer(culture);

        var result = localizer.Localize(code, "fallback", "abc");

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("en", "ProblemDetails.ValidationTitle", "One or more validation errors occurred")]
    [InlineData("fr", "ProblemDetails.ValidationTitle", "Une ou plusieurs erreurs de validation se sont produites")]
    [InlineData("en", "ProblemDetails.NotFound", "Not Found")]
    [InlineData("fr", "ProblemDetails.NotFound", "Non trouvé")]
    [InlineData("en", "ProblemDetails.Conflict", "Conflict")]
    [InlineData("fr", "ProblemDetails.Conflict", "Conflit")]
    [InlineData("en", "ProblemDetails.UnexpectedTitle", "An unexpected error occurred")]
    [InlineData("fr", "ProblemDetails.UnexpectedTitle", "Une erreur inattendue s'est produite")]
    public void LocalizeByKey_ProblemDetails_ReturnsCorrectTranslation(string culture, string key, string expected)
    {
        var localizer = CreateLocalizer(culture);

        var result = localizer.LocalizeByKey(key);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("en", "ShoppingItem.NotFound", "Shopping item with ID 123 was not found.")]
    [InlineData("fr", "ShoppingItem.NotFound", "L'article de la liste de courses avec l'identifiant 123 n'a pas été trouvé.")]
    [InlineData("en", "Recipe.NotFound", "Recipe with ID 456 was not found.")]
    [InlineData("fr", "Recipe.NotFound", "La recette avec l'identifiant 456 n'a pas été trouvée.")]
    public void LocalizeByKey_WithArgs_ReturnsFormattedTranslation(string culture, string key, string expected)
    {
        var localizer = CreateLocalizer(culture);

        var result = localizer.LocalizeByKey(key, key.Contains("Shopping") ? "123" : "456");

        result.Should().Be(expected);
    }

    [Fact]
    public void LocalizeByKey_WithUnknownKey_ReturnsKey()
    {
        var localizer = CreateLocalizer("en");

        var result = localizer.LocalizeByKey("Unknown.Key");

        result.Should().Be("Unknown.Key");
    }

    [Theory]
    [InlineData("en", "Admin.UsernameAlreadyExists", "A user with this username already exists.")]
    [InlineData("fr", "Admin.UsernameAlreadyExists", "Un utilisateur avec ce nom d'utilisateur existe déjà.")]
    [InlineData("en", "Admin.PasswordRequirementsNotMet", "The password does not meet the requirements.")]
    [InlineData("fr", "Admin.PasswordRequirementsNotMet", "Le mot de passe ne respecte pas les exigences requises.")]
    public void Localize_AdminErrors_ReturnsCorrectTranslation(string culture, string code, string expected)
    {
        var localizer = CreateLocalizer(culture);

        var result = localizer.Localize(code, "fallback");

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("en", "Meal.SlotAlreadyFilled", "A meal is already planned for Lunch on 2026-01-15. Use swap instead.")]
    [InlineData("fr", "Meal.SlotAlreadyFilled", "Un repas est déjà planifié pour Lunch le 2026-01-15. Utilisez l'échange à la place.")]
    public void Localize_SlotAlreadyFilled_ReturnsFormattedTranslation(string culture, string code, string expected)
    {
        var localizer = CreateLocalizer(culture);

        var result = localizer.Localize(code, "fallback", "Lunch", "2026-01-15");

        result.Should().Be(expected);
    }
}
