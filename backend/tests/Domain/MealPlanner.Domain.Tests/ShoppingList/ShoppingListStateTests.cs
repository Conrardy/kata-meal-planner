using FluentAssertions;
using MealPlanner.Domain.ShoppingList;
using Xunit;

namespace MealPlanner.Domain.Tests.ShoppingList;

public sealed class ShoppingListStateTests
{
    [Fact]
    public void Constructor_ShouldSetStartDateAndEndDate()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var state = new ShoppingListState(startDate);

        // Assert
        state.StartDate.Should().Be(startDate);
        state.EndDate.Should().Be(startDate.AddDays(6));
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyCollections()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var state = new ShoppingListState(startDate);

        // Assert
        state.CheckedItems.Should().BeEmpty();
        state.CustomItems.Should().BeEmpty();
    }

    [Fact]
    public void SetItemChecked_ShouldAddItemToCheckedItems()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));

        // Act
        state.SetItemChecked("item-1", true);

        // Assert
        state.CheckedItems.Should().ContainKey("item-1");
        state.CheckedItems["item-1"].Should().BeTrue();
    }

    [Fact]
    public void SetItemChecked_WithFalse_ShouldAddItemAsUnchecked()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));

        // Act
        state.SetItemChecked("item-1", false);

        // Assert
        state.CheckedItems.Should().ContainKey("item-1");
        state.CheckedItems["item-1"].Should().BeFalse();
    }

    [Fact]
    public void SetItemChecked_ShouldUpdateExistingItem()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));
        state.SetItemChecked("item-1", false);

        // Act
        state.SetItemChecked("item-1", true);

        // Assert
        state.CheckedItems["item-1"].Should().BeTrue();
    }

    [Fact]
    public void IsItemChecked_WithCheckedItem_ShouldReturnTrue()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));
        state.SetItemChecked("item-1", true);

        // Act
        var result = state.IsItemChecked("item-1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsItemChecked_WithUncheckedItem_ShouldReturnFalse()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));
        state.SetItemChecked("item-1", false);

        // Act
        var result = state.IsItemChecked("item-1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsItemChecked_WithNonExistentItem_ShouldReturnFalse()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = state.IsItemChecked("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AddCustomItem_ShouldCreateItemWithGeneratedId()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));

        // Act
        var item = state.AddCustomItem("Test Item", "2", "cups", ItemCategory.Pantry);

        // Assert
        item.Id.Should().StartWith("custom-");
        item.Name.Should().Be("Test Item");
        item.Quantity.Should().Be("2");
        item.Unit.Should().Be("cups");
        item.Category.Should().Be(ItemCategory.Pantry);
        item.IsCustom.Should().BeTrue();
        item.IsChecked.Should().BeFalse();
    }

    [Fact]
    public void AddCustomItem_ShouldAddItemToCustomItems()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));

        // Act
        state.AddCustomItem("Test Item", "1", null, ItemCategory.Dairy);

        // Assert
        state.CustomItems.Should().HaveCount(1);
        state.CustomItems[0].Name.Should().Be("Test Item");
    }

    [Fact]
    public void AddCustomItemWithId_ShouldUseProvidedId()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));
        var customId = "custom-my-id";

        // Act
        var item = state.AddCustomItemWithId(customId, "Test Item", "1", null, ItemCategory.Produce);

        // Assert
        item.Id.Should().Be(customId);
    }

    [Fact]
    public void RemoveCustomItem_WithExistingItem_ShouldReturnTrueAndRemove()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));
        var item = state.AddCustomItem("Test Item", "1", null, ItemCategory.Pantry);
        state.SetItemChecked(item.Id, true);

        // Act
        var result = state.RemoveCustomItem(item.Id);

        // Assert
        result.Should().BeTrue();
        state.CustomItems.Should().BeEmpty();
        state.CheckedItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCustomItem_WithNonExistentItem_ShouldReturnFalse()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));

        // Act
        var result = state.RemoveCustomItem("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveItem_WithCustomItemId_ShouldRemoveCustomItem()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));
        var item = state.AddCustomItem("Test Item", "1", null, ItemCategory.Pantry);

        // Act
        var result = state.RemoveItem(item.Id);

        // Assert
        result.Should().BeTrue();
        state.CustomItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WithRegularItemId_ShouldRemoveFromCheckedItems()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));
        state.SetItemChecked("regular-item", true);

        // Act
        var result = state.RemoveItem("regular-item");

        // Assert
        result.Should().BeTrue();
        state.CheckedItems.Should().NotContainKey("regular-item");
    }

    [Fact]
    public void AddCustomItem_MultipleItems_ShouldAllHaveUniqueIds()
    {
        // Arrange
        var state = new ShoppingListState(DateOnly.FromDateTime(DateTime.Now));

        // Act
        var item1 = state.AddCustomItem("Item 1", "1", null, ItemCategory.Pantry);
        var item2 = state.AddCustomItem("Item 2", "2", null, ItemCategory.Pantry);
        var item3 = state.AddCustomItem("Item 3", "3", null, ItemCategory.Pantry);

        // Assert
        item1.Id.Should().NotBe(item2.Id);
        item2.Id.Should().NotBe(item3.Id);
        item1.Id.Should().NotBe(item3.Id);
    }
}
