using MediatR;
using MealPlanner.Domain.Meals;
using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Application.ShoppingList;

public sealed record GenerateShoppingListQuery(DateOnly StartDate) : IRequest<ShoppingListDto>;

public sealed class GenerateShoppingListQueryHandler : IRequestHandler<GenerateShoppingListQuery, ShoppingListDto>
{
    private readonly IPlannedMealRepository _mealRepository;
    private readonly IShoppingListStateRepository _stateRepository;
    private readonly IShoppingListSyncService _syncService;
    private readonly IQuantityParser _quantityParser;

    public GenerateShoppingListQueryHandler(
        IPlannedMealRepository mealRepository,
        IShoppingListStateRepository stateRepository,
        IShoppingListSyncService syncService,
        IQuantityParser quantityParser)
    {
        _mealRepository = mealRepository;
        _stateRepository = stateRepository;
        _syncService = syncService;
        _quantityParser = quantityParser;
    }

    public async Task<ShoppingListDto> Handle(GenerateShoppingListQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.StartDate.AddDays(6);
        var meals = await _mealRepository.GetByDateRangeAsync(request.StartDate, endDate, cancellationToken);
        var state = await _stateRepository.GetOrCreateAsync(request.StartDate, cancellationToken);

        var hadPendingChanges = _syncService.HasPendingChanges(request.StartDate);
        var pendingChangeCount = _syncService.GetPendingChangeCount(request.StartDate);

        var aggregatedItems = AggregateIngredients(meals);
        var categorizedItems = CategorizeItems(aggregatedItems, state);

        _syncService.MarkSynced(request.StartDate);

        var updateNotice = hadPendingChanges
            ? $"Shopping list updated with {pendingChangeCount} meal plan change{(pendingChangeCount > 1 ? "s" : "")}"
            : null;

        return new ShoppingListDto(
            request.StartDate,
            endDate,
            categorizedItems,
            WasUpdated: hadPendingChanges,
            UpdateNotice: updateNotice);
    }

    private Dictionary<string, AggregatedIngredient> AggregateIngredients(IReadOnlyList<PlannedMeal> meals)
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
                    var combinedQuantity = _quantityParser.Combine(
                        existing.Quantity,
                        existing.Unit ?? string.Empty,
                        ingredient.Quantity,
                        ingredient.Unit ?? string.Empty);
                    aggregated[key] = existing with { Quantity = combinedQuantity };
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

    private static IReadOnlyList<ShoppingCategoryDto> CategorizeItems(
        Dictionary<string, AggregatedIngredient> items,
        ShoppingListState state)
    {
        var categoryOrder = new[] { ItemCategory.Produce, ItemCategory.Dairy, ItemCategory.Meat, ItemCategory.Pantry };

        return categoryOrder
            .Select(category =>
            {
                var mealPlanItems = items.Values
                    .Where(i => i.Category == category)
                    .Select(i => new ShoppingItemDto(
                        Id: GenerateItemId(i.Name),
                        Name: i.Name,
                        Quantity: i.Quantity,
                        Unit: i.Unit,
                        IsChecked: state.IsItemChecked(GenerateItemId(i.Name)),
                        IsCustom: false))
                    .ToList();

                var customItems = state.CustomItems
                    .Where(i => i.Category == category)
                    .Select(i => new ShoppingItemDto(
                        Id: i.Id,
                        Name: i.Name,
                        Quantity: i.Quantity,
                        Unit: i.Unit,
                        IsChecked: state.IsItemChecked(i.Id),
                        IsCustom: true))
                    .ToList();

                var allItems = mealPlanItems
                    .Concat(customItems)
                    .OrderBy(i => i.Name)
                    .ToList();

                return new ShoppingCategoryDto(category.Value, allItems);
            })
            .Where(c => c.Items.Count > 0)
            .ToList();
    }

    private static string GenerateItemId(string name) =>
        $"item-{name.ToLowerInvariant().Replace(" ", "-")}";

    private sealed record AggregatedIngredient(string Name, string Quantity, string? Unit, ItemCategory Category);
}
