namespace MealPlanner.Domain.ShoppingList;

/// <summary>
/// Service interface for tracking synchronization state between meal plans and shopping lists.
/// </summary>
public interface IShoppingListSyncService
{
    /// <summary>
    /// Marks the shopping list as requiring sync for the given date range.
    /// </summary>
    void MarkPendingSync(DateOnly affectedDate);

    /// <summary>
    /// Gets the last sync timestamp for a date range, or null if no sync has occurred.
    /// </summary>
    DateTime? GetLastSyncTimestamp(DateOnly startDate);

    /// <summary>
    /// Marks the shopping list as synced for the given start date.
    /// </summary>
    void MarkSynced(DateOnly startDate);

    /// <summary>
    /// Checks if there are pending changes that need to be synced.
    /// </summary>
    bool HasPendingChanges(DateOnly startDate);

    /// <summary>
    /// Gets the count of pending changes since last sync.
    /// </summary>
    int GetPendingChangeCount(DateOnly startDate);
}
