using MealPlanner.Domain.ShoppingList;

namespace MealPlanner.Infrastructure.Services;

/// <summary>
/// In-memory implementation of shopping list sync tracking.
/// Tracks when meal plan changes occur and manages sync state per date range.
/// </summary>
public sealed class InMemoryShoppingListSyncService : IShoppingListSyncService
{
    private readonly Dictionary<DateOnly, SyncState> _syncStates = new();
    private readonly object _lock = new();

    public void MarkPendingSync(DateOnly affectedDate)
    {
        lock (_lock)
        {
            var weekStart = GetWeekStart(affectedDate);

            if (_syncStates.TryGetValue(weekStart, out var state))
            {
                state.PendingChangeCount++;
                state.LastChangeTimestamp = DateTime.UtcNow;
            }
            else
            {
                _syncStates[weekStart] = new SyncState
                {
                    PendingChangeCount = 1,
                    LastChangeTimestamp = DateTime.UtcNow,
                    LastSyncTimestamp = null
                };
            }
        }
    }

    public DateTime? GetLastSyncTimestamp(DateOnly startDate)
    {
        lock (_lock)
        {
            var weekStart = GetWeekStart(startDate);
            return _syncStates.TryGetValue(weekStart, out var state) ? state.LastSyncTimestamp : null;
        }
    }

    public void MarkSynced(DateOnly startDate)
    {
        lock (_lock)
        {
            var weekStart = GetWeekStart(startDate);

            if (_syncStates.TryGetValue(weekStart, out var state))
            {
                state.PendingChangeCount = 0;
                state.LastSyncTimestamp = DateTime.UtcNow;
            }
            else
            {
                _syncStates[weekStart] = new SyncState
                {
                    PendingChangeCount = 0,
                    LastChangeTimestamp = null,
                    LastSyncTimestamp = DateTime.UtcNow
                };
            }
        }
    }

    public bool HasPendingChanges(DateOnly startDate)
    {
        lock (_lock)
        {
            var weekStart = GetWeekStart(startDate);
            return _syncStates.TryGetValue(weekStart, out var state) && state.PendingChangeCount > 0;
        }
    }

    public int GetPendingChangeCount(DateOnly startDate)
    {
        lock (_lock)
        {
            var weekStart = GetWeekStart(startDate);
            return _syncStates.TryGetValue(weekStart, out var state) ? state.PendingChangeCount : 0;
        }
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var daysFromSunday = (int)date.DayOfWeek;
        return date.AddDays(-daysFromSunday);
    }

    private sealed class SyncState
    {
        public int PendingChangeCount { get; set; }
        public DateTime? LastChangeTimestamp { get; set; }
        public DateTime? LastSyncTimestamp { get; set; }
    }
}
