using MediatR;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Application.ShoppingList;

public sealed record GenerateShoppingListQuery(DateOnly StartDate) : IRequest<ShoppingListDto>;

public sealed class GenerateShoppingListQueryHandler : IRequestHandler<GenerateShoppingListQuery, ShoppingListDto>
{
    private readonly IPlannedMealRepository _repository;

    public GenerateShoppingListQueryHandler(IPlannedMealRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShoppingListDto> Handle(GenerateShoppingListQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.StartDate.AddDays(6);
        var meals = await _repository.GetByDateRangeAsync(request.StartDate, endDate, cancellationToken);

        var aggregatedItems = AggregateIngredients(meals);
        var categorizedItems = CategorizeItems(aggregatedItems);

        return new ShoppingListDto(request.StartDate, endDate, categorizedItems);
    }

    private static Dictionary<string, AggregatedIngredient> AggregateIngredients(IReadOnlyList<PlannedMeal> meals)
    {
        var aggregated = new Dictionary<string, AggregatedIngredient>(StringComparer.OrdinalIgnoreCase);

        foreach (var meal in meals)
        {
            if (meal.Recipe?.Ingredients == null) continue;

            foreach (var ingredient in meal.Recipe.Ingredients)
            {
                var key = NormalizeIngredientName(ingredient.Name);

                if (aggregated.TryGetValue(key, out var existing))
                {
                    var combined = CombineQuantities(existing, ingredient.Quantity, ingredient.Unit);
                    aggregated[key] = combined;
                }
                else
                {
                    aggregated[key] = new AggregatedIngredient(
                        ingredient.Name,
                        ingredient.Quantity,
                        ingredient.Unit,
                        ItemCategory.FromIngredient(ingredient.Name)
                    );
                }
            }
        }

        return aggregated;
    }

    private static string NormalizeIngredientName(string name)
    {
        return name.ToLowerInvariant().Trim();
    }

    private static AggregatedIngredient CombineQuantities(AggregatedIngredient existing, string newQuantity, string? newUnit)
    {
        if (TryParseQuantity(existing.Quantity, out var existingNum) &&
            TryParseQuantity(newQuantity, out var newNum) &&
            existing.Unit == newUnit)
        {
            var totalQuantity = existingNum + newNum;
            return existing with { Quantity = FormatQuantity(totalQuantity) };
        }

        return existing with { Quantity = $"{existing.Quantity} + {newQuantity}" };
    }

    private static bool TryParseQuantity(string quantity, out double result)
    {
        result = 0;

        if (quantity.Contains('/'))
        {
            var parts = quantity.Split('/');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var numerator) &&
                double.TryParse(parts[1], out var denominator) &&
                denominator != 0)
            {
                result = numerator / denominator;
                return true;
            }
            return false;
        }

        if (quantity.ToLowerInvariant().Contains("to taste"))
        {
            return false;
        }

        return double.TryParse(quantity, out result);
    }

    private static string FormatQuantity(double quantity)
    {
        if (Math.Abs(quantity - Math.Round(quantity)) < 0.01)
        {
            return ((int)Math.Round(quantity)).ToString();
        }
        return quantity.ToString("0.##");
    }

    private static IReadOnlyList<ShoppingCategoryDto> CategorizeItems(Dictionary<string, AggregatedIngredient> items)
    {
        var categoryOrder = new[] { ItemCategory.Produce, ItemCategory.Dairy, ItemCategory.Meat, ItemCategory.Pantry };

        return categoryOrder
            .Select(category =>
            {
                var categoryItems = items.Values
                    .Where(i => i.Category == category)
                    .Select(i => new ShoppingItemDto(i.Name, i.Quantity, i.Unit))
                    .OrderBy(i => i.Name)
                    .ToList();

                return new ShoppingCategoryDto(category.Value, categoryItems);
            })
            .Where(c => c.Items.Count > 0)
            .ToList();
    }

    private sealed record AggregatedIngredient(string Name, string Quantity, string? Unit, ItemCategory Category);
}
