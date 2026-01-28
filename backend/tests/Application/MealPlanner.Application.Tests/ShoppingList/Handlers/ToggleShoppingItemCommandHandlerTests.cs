using FluentAssertions;
using MealPlanner.Application.ShoppingList;
using MealPlanner.Domain.ShoppingList;
using Xunit;

namespace MealPlanner.Application.Tests.ShoppingList.Handlers;

public sealed class ToggleShoppingItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldToggleItemToChecked()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new ToggleShoppingItemCommandHandler(repository);
        var command = new ToggleShoppingItemCommand(startDate, "item-test", IsChecked: true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.SavedState.Should().NotBeNull();
        repository.SavedState!.IsItemChecked("item-test").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithFalseIsChecked_ShouldToggleItemToUnchecked()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var state = await repository.GetOrCreateAsync(startDate);
        state.SetItemChecked("item-test", true);
        await repository.SaveAsync(state);

        var handler = new ToggleShoppingItemCommandHandler(repository);
        var command = new ToggleShoppingItemCommand(startDate, "item-test", IsChecked: false);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.SavedState.Should().NotBeNull();
        repository.SavedState!.IsItemChecked("item-test").Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentItem_ShouldCreateCheckedItem()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new ToggleShoppingItemCommandHandler(repository);
        var command = new ToggleShoppingItemCommand(startDate, "new-item", IsChecked: true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.SavedState.Should().NotBeNull();
        repository.SavedState!.IsItemChecked("new-item").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSaveStateToRepository()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new ToggleShoppingItemCommandHandler(repository);
        var command = new ToggleShoppingItemCommand(startDate, "item-test", IsChecked: true);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.SavedState.Should().NotBeNull();
        repository.SavedState!.StartDate.Should().Be(startDate);
    }

    [Fact]
    public async Task Handle_TogglingMultipleTimes_ShouldUpdateState()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new ToggleShoppingItemCommandHandler(repository);

        // Act
        await handler.Handle(new ToggleShoppingItemCommand(startDate, "item-test", true), CancellationToken.None);
        repository.SavedState!.IsItemChecked("item-test").Should().BeTrue();

        await handler.Handle(new ToggleShoppingItemCommand(startDate, "item-test", false), CancellationToken.None);
        repository.SavedState!.IsItemChecked("item-test").Should().BeFalse();

        await handler.Handle(new ToggleShoppingItemCommand(startDate, "item-test", true), CancellationToken.None);

        // Assert
        repository.SavedState!.IsItemChecked("item-test").Should().BeTrue();
    }

    private sealed class InMemoryShoppingListStateRepository : IShoppingListStateRepository
    {
        private readonly Dictionary<DateOnly, ShoppingListState> _states = [];
        public ShoppingListState? SavedState { get; private set; }

        public Task<ShoppingListState> GetOrCreateAsync(DateOnly startDate, CancellationToken cancellationToken = default)
        {
            if (!_states.TryGetValue(startDate, out var state))
            {
                state = new ShoppingListState(startDate);
                _states[startDate] = state;
            }
            return Task.FromResult(state);
        }

        public Task SaveAsync(ShoppingListState state, CancellationToken cancellationToken = default)
        {
            SavedState = state;
            _states[state.StartDate] = state;
            return Task.CompletedTask;
        }
    }
}
