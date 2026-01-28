using FluentAssertions;
using MealPlanner.Application.ShoppingList;
using MealPlanner.Domain.ShoppingList;
using Xunit;

namespace MealPlanner.Application.Tests.ShoppingList.Handlers;

public sealed class RemoveShoppingItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingCustomItem_ShouldReturnTrueAndRemoveItem()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var state = await repository.GetOrCreateAsync(startDate);
        var customItem = state.AddCustomItem("Custom Item", "1", "unit", ItemCategory.Pantry);
        await repository.SaveAsync(state);

        var handler = new RemoveShoppingItemCommandHandler(repository);
        var command = new RemoveShoppingItemCommand(startDate, customItem.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        repository.SavedState.Should().NotBeNull();
        repository.SavedState!.CustomItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomItem_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new RemoveShoppingItemCommandHandler(repository);
        var command = new RemoveShoppingItemCommand(startDate, "custom-non-existent");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentRegularItem_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new RemoveShoppingItemCommandHandler(repository);
        var command = new RemoveShoppingItemCommand(startDate, "item-non-existent");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithCheckedItem_ShouldRemoveFromCheckedItems()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var state = await repository.GetOrCreateAsync(startDate);
        state.SetItemChecked("item-test", true);
        await repository.SaveAsync(state);

        var handler = new RemoveShoppingItemCommandHandler(repository);
        var command = new RemoveShoppingItemCommand(startDate, "item-test");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        repository.SavedState!.IsItemChecked("item-test").Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSaveStateToRepository()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var state = await repository.GetOrCreateAsync(startDate);
        var customItem = state.AddCustomItem("Test Item", "1", null, ItemCategory.Pantry);
        await repository.SaveAsync(state);

        var handler = new RemoveShoppingItemCommandHandler(repository);
        var command = new RemoveShoppingItemCommand(startDate, customItem.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.SavedState.Should().NotBeNull();
        repository.SavedState!.StartDate.Should().Be(startDate);
    }

    [Fact]
    public async Task Handle_RemovingMultipleItems_ShouldRemoveEachItem()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var state = await repository.GetOrCreateAsync(startDate);
        var item1 = state.AddCustomItem("Item 1", "1", null, ItemCategory.Pantry);
        var item2 = state.AddCustomItem("Item 2", "2", null, ItemCategory.Pantry);
        var item3 = state.AddCustomItem("Item 3", "3", null, ItemCategory.Pantry);
        await repository.SaveAsync(state);

        var handler = new RemoveShoppingItemCommandHandler(repository);

        // Act
        var result1 = await handler.Handle(new RemoveShoppingItemCommand(startDate, item1.Id), CancellationToken.None);
        var result2 = await handler.Handle(new RemoveShoppingItemCommand(startDate, item2.Id), CancellationToken.None);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        repository.SavedState!.CustomItems.Should().HaveCount(1);
        repository.SavedState.CustomItems.Should().Contain(i => i.Id == item3.Id);
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
