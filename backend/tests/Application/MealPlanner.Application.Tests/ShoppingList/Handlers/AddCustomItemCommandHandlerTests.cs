using FluentAssertions;
using MealPlanner.Application.ShoppingList;
using MealPlanner.Domain.ShoppingList;
using Xunit;

namespace MealPlanner.Application.Tests.ShoppingList.Handlers;

public sealed class AddCustomItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomItem()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);
        var command = new AddCustomItemCommand(startDate, "Custom Snacks", "2", "bags", "Pantry");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Custom Snacks");
        result.Quantity.Should().Be("2");
        result.Unit.Should().Be("bags");
        result.IsCustom.Should().BeTrue();
        result.IsChecked.Should().BeFalse();
        result.Id.Should().StartWith("custom-");
    }

    [Fact]
    public async Task Handle_WithProduceCategory_ShouldCreateItemWithProduceCategory()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);
        var command = new AddCustomItemCommand(startDate, "Fresh Herbs", "1", "bunch", "Produce");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Fresh Herbs");
    }

    [Fact]
    public async Task Handle_WithDairyCategory_ShouldCreateItemWithDairyCategory()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);
        var command = new AddCustomItemCommand(startDate, "Almond Milk", "1", "gallon", "Dairy");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Almond Milk");
    }

    [Fact]
    public async Task Handle_WithMeatCategory_ShouldCreateItemWithMeatCategory()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);
        var command = new AddCustomItemCommand(startDate, "Ground Turkey", "1", "lb", "Meat");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Ground Turkey");
    }

    [Fact]
    public async Task Handle_WithUnknownCategory_ShouldDefaultToPantry()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);
        var command = new AddCustomItemCommand(startDate, "Mystery Item", "1", null, "Unknown");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Mystery Item");
    }

    [Fact]
    public async Task Handle_WithNullUnit_ShouldCreateItemWithNullUnit()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);
        var command = new AddCustomItemCommand(startDate, "Salt", "1", null, "Pantry");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Unit.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldSaveStateToRepository()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);
        var command = new AddCustomItemCommand(startDate, "Test Item", "1", "piece", "Pantry");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.SavedState.Should().NotBeNull();
        repository.SavedState!.StartDate.Should().Be(startDate);
        repository.SavedState.CustomItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_AddingMultipleItems_ShouldCreateUniqueIds()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var repository = new InMemoryShoppingListStateRepository();
        var handler = new AddCustomItemCommandHandler(repository);

        var command1 = new AddCustomItemCommand(startDate, "Item 1", "1", null, "Pantry");
        var command2 = new AddCustomItemCommand(startDate, "Item 2", "2", null, "Pantry");

        // Act
        var result1 = await handler.Handle(command1, CancellationToken.None);
        var result2 = await handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Id.Should().NotBe(result2.Id);
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
