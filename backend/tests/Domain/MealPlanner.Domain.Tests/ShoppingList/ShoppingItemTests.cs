using FluentAssertions;
using MealPlanner.Domain.ShoppingList;
using Xunit;

namespace MealPlanner.Domain.Tests.ShoppingList;

public sealed class ShoppingItemTests
{
    [Fact]
    public void Constructor_WithRequiredProperties_ShouldCreateShoppingItem()
    {
        // Arrange & Act
        var item = new ShoppingItem(
            Id: "item-1",
            Name: "Milk",
            Quantity: "1",
            Unit: "gallon",
            Category: ItemCategory.Dairy
        );

        // Assert
        item.Id.Should().Be("item-1");
        item.Name.Should().Be("Milk");
        item.Quantity.Should().Be("1");
        item.Unit.Should().Be("gallon");
        item.Category.Should().Be(ItemCategory.Dairy);
        item.IsChecked.Should().BeFalse();
        item.IsCustom.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithCheckedTrue_ShouldSetIsChecked()
    {
        // Arrange & Act
        var item = new ShoppingItem(
            Id: "item-1",
            Name: "Eggs",
            Quantity: "12",
            Unit: null,
            Category: ItemCategory.Dairy,
            IsChecked: true
        );

        // Assert
        item.IsChecked.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCustomTrue_ShouldSetIsCustom()
    {
        // Arrange & Act
        var item = new ShoppingItem(
            Id: "custom-1",
            Name: "Custom Item",
            Quantity: "1",
            Unit: null,
            Category: ItemCategory.Pantry,
            IsCustom: true
        );

        // Assert
        item.IsCustom.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullUnit_ShouldAllowNullUnit()
    {
        // Arrange & Act
        var item = new ShoppingItem(
            Id: "item-1",
            Name: "Salt",
            Quantity: "1",
            Unit: null,
            Category: ItemCategory.Pantry
        );

        // Assert
        item.Unit.Should().BeNull();
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var item1 = new ShoppingItem("id", "Flour", "2", "cups", ItemCategory.Pantry);
        var item2 = new ShoppingItem("id", "Flour", "2", "cups", ItemCategory.Pantry);

        // Assert
        item1.Should().Be(item2);
    }

    [Fact]
    public void With_ShouldCreateModifiedCopy()
    {
        // Arrange
        var original = new ShoppingItem("id", "Flour", "2", "cups", ItemCategory.Pantry);

        // Act
        var modified = original with { IsChecked = true };

        // Assert
        original.IsChecked.Should().BeFalse();
        modified.IsChecked.Should().BeTrue();
        modified.Name.Should().Be("Flour");
    }
}

public sealed class ItemCategoryTests
{
    [Fact]
    public void Produce_ShouldHaveCorrectValue()
    {
        ItemCategory.Produce.Value.Should().Be("Produce");
    }

    [Fact]
    public void Dairy_ShouldHaveCorrectValue()
    {
        ItemCategory.Dairy.Value.Should().Be("Dairy");
    }

    [Fact]
    public void Meat_ShouldHaveCorrectValue()
    {
        ItemCategory.Meat.Value.Should().Be("Meat");
    }

    [Fact]
    public void Pantry_ShouldHaveCorrectValue()
    {
        ItemCategory.Pantry.Value.Should().Be("Pantry");
    }

    [Theory]
    [InlineData("lettuce", "Produce")]
    [InlineData("tomato", "Produce")]
    [InlineData("cucumber", "Produce")]
    [InlineData("onion", "Produce")]
    [InlineData("garlic", "Produce")]
    [InlineData("berries", "Produce")]
    [InlineData("strawberry", "Produce")]
    [InlineData("avocado", "Produce")]
    [InlineData("lemon", "Produce")]
    [InlineData("asparagus", "Produce")]
    [InlineData("broccoli", "Produce")]
    [InlineData("bell pepper", "Produce")]
    [InlineData("ginger", "Produce")]
    [InlineData("fresh dill", "Produce")]
    [InlineData("mixed greens", "Produce")]
    [InlineData("tropical fruit", "Produce")]
    public void FromIngredient_ProduceItems_ShouldReturnProduce(string ingredient, string expectedCategory)
    {
        // Act
        var category = ItemCategory.FromIngredient(ingredient);

        // Assert
        category.Value.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData("milk", "Dairy")]
    [InlineData("whole milk", "Dairy")]
    [InlineData("cheddar cheese", "Dairy")]
    [InlineData("parmesan", "Dairy")]
    [InlineData("butter", "Dairy")]
    [InlineData("greek yogurt", "Dairy")]
    [InlineData("heavy cream", "Dairy")]
    [InlineData("eggs", "Dairy")]
    public void FromIngredient_DairyItems_ShouldReturnDairy(string ingredient, string expectedCategory)
    {
        // Act
        var category = ItemCategory.FromIngredient(ingredient);

        // Assert
        category.Value.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData("chicken breast", "Meat")]
    [InlineData("ground beef", "Meat")]
    [InlineData("salmon fillet", "Meat")]
    [InlineData("white fish", "Meat")]
    [InlineData("pork chops", "Meat")]
    [InlineData("deli meat", "Meat")]
    [InlineData("tofu", "Meat")]
    public void FromIngredient_MeatItems_ShouldReturnMeat(string ingredient, string expectedCategory)
    {
        // Act
        var category = ItemCategory.FromIngredient(ingredient);

        // Assert
        category.Value.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData("flour", "Pantry")]
    [InlineData("sugar", "Pantry")]
    [InlineData("salt", "Pantry")]
    [InlineData("olive oil", "Pantry")]
    [InlineData("pasta", "Pantry")]
    [InlineData("rice", "Pantry")]
    [InlineData("red pepper flakes", "Pantry")]
    public void FromIngredient_PantryItems_ShouldReturnPantry(string ingredient, string expectedCategory)
    {
        // Act
        var category = ItemCategory.FromIngredient(ingredient);

        // Assert
        category.Value.Should().Be(expectedCategory);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        ItemCategory.Produce.ToString().Should().Be("Produce");
        ItemCategory.Dairy.ToString().Should().Be("Dairy");
        ItemCategory.Meat.ToString().Should().Be("Meat");
        ItemCategory.Pantry.ToString().Should().Be("Pantry");
    }

    [Fact]
    public void FromIngredient_CaseInsensitive_ShouldWork()
    {
        // Act & Assert
        ItemCategory.FromIngredient("MILK").Should().Be(ItemCategory.Dairy);
        ItemCategory.FromIngredient("Chicken").Should().Be(ItemCategory.Meat);
        ItemCategory.FromIngredient("LETTUCE").Should().Be(ItemCategory.Produce);
    }
}
