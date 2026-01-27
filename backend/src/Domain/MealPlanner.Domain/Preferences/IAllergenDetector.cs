using MealPlanner.Domain.Recipes;

namespace MealPlanner.Domain.Preferences;

public interface IAllergenDetector
{
    bool ContainsAllergen(Recipe recipe, IReadOnlyList<Allergy> allergies);
}
