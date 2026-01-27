namespace MealPlanner.Domain.Preferences;

public static class AllergenKeywords
{
    public static readonly IReadOnlyList<string> Dairy = new[]
    {
        "milk", "cheese", "cream", "butter", "yogurt", "parmesan", "dairy"
    };

    public static readonly IReadOnlyList<string> Nuts = new[]
    {
        "nut", "almond", "walnut", "pecan", "cashew", "pistachio", "hazelnut", "peanut"
    };

    public static readonly IReadOnlyList<string> Eggs = new[]
    {
        "egg", "eggs"
    };

    public static readonly IReadOnlyList<string> Shellfish = new[]
    {
        "shrimp", "lobster", "crab", "oyster", "mussel", "clam", "scallop", "shellfish"
    };

    public static readonly IReadOnlyList<string> Soy = new[]
    {
        "soy", "tofu", "edamame", "tempeh", "miso"
    };
}
