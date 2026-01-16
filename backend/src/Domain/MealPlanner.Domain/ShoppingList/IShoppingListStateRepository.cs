namespace MealPlanner.Domain.ShoppingList;

public interface IShoppingListStateRepository
{
    Task<ShoppingListState> GetOrCreateAsync(DateOnly startDate, CancellationToken cancellationToken = default);
    Task SaveAsync(ShoppingListState state, CancellationToken cancellationToken = default);
}

public sealed class ShoppingListState
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    private readonly Dictionary<string, bool> _checkedItems = new();
    private readonly List<ShoppingItem> _customItems = [];

    public ShoppingListState(DateOnly startDate)
    {
        StartDate = startDate;
        EndDate = startDate.AddDays(6);
    }

    public IReadOnlyDictionary<string, bool> CheckedItems => _checkedItems;
    public IReadOnlyList<ShoppingItem> CustomItems => _customItems.AsReadOnly();

    public void SetItemChecked(string itemId, bool isChecked)
    {
        _checkedItems[itemId] = isChecked;
    }

    public bool IsItemChecked(string itemId)
    {
        return _checkedItems.TryGetValue(itemId, out var isChecked) && isChecked;
    }

    public ShoppingItem AddCustomItem(string name, string quantity, string? unit, ItemCategory category)
    {
        var id = $"custom-{Guid.NewGuid():N}";
        var item = new ShoppingItem(id, name, quantity, unit, category, IsChecked: false, IsCustom: true);
        _customItems.Add(item);
        return item;
    }

    public bool RemoveCustomItem(string itemId)
    {
        var item = _customItems.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return false;
        _customItems.Remove(item);
        _checkedItems.Remove(itemId);
        return true;
    }

    public bool RemoveItem(string itemId)
    {
        if (itemId.StartsWith("custom-"))
        {
            return RemoveCustomItem(itemId);
        }
        _checkedItems.Remove(itemId);
        return true;
    }
}
