using MealPlanner.Domain.Preferences;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Infrastructure.Services;

public sealed class AllergenDetector : IAllergenDetector
{
    public bool ContainsAllergen(Recipe recipe, IReadOnlyList<Allergy> allergies)
    {
        foreach (var allergy in allergies)
        {
            if (allergy == Allergy.Gluten && !recipe.Tags.Contains("Gluten-Free", StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            if (allergy == Allergy.Dairy && recipe.Ingredients.Any(i => ContainsKeywords(i.Name, AllergenKeywords.Dairy)))
            {
                return true;
            }
            if (allergy == Allergy.Nuts && recipe.Ingredients.Any(i => ContainsKeywords(i.Name, AllergenKeywords.Nuts)))
            {
                return true;
            }
            if (allergy == Allergy.Eggs && recipe.Ingredients.Any(i => ContainsKeywords(i.Name, AllergenKeywords.Eggs)))
            {
                return true;
            }
            if (allergy == Allergy.Shellfish && recipe.Ingredients.Any(i => ContainsKeywords(i.Name, AllergenKeywords.Shellfish)))
            {
                return true;
            }
            if (allergy == Allergy.Soy && recipe.Ingredients.Any(i => ContainsKeywords(i.Name, AllergenKeywords.Soy)))
            {
                return true;
            }
        }
        return false;
    }

    private static bool ContainsKeywords(string ingredientName, IReadOnlyList<string> keywords)
    {
        return keywords.Any(k => ingredientName.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
