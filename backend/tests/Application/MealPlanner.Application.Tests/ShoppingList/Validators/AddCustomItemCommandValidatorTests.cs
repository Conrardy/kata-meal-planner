using FluentAssertions;
using MealPlanner.Application.ShoppingList;
using MealPlanner.Application.ShoppingList.Validators;
using Xunit;

namespace MealPlanner.Application.Tests.ShoppingList.Validators;

public sealed class AddCustomItemCommandValidatorTests
{
    private readonly AddCustomItemCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new AddCustomItemCommand(
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            Name: "Milk",
            Quantity: "1",
            Unit: "gallon",
            Category: "Dairy"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyName_ShouldFail(string? name)
    {
        // Arrange
        var command = new AddCustomItemCommand(
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            Name: name!,
            Quantity: "1",
            Unit: null,
            Category: "Pantry"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var command = new AddCustomItemCommand(
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            Name: new string('a', 101),
            Quantity: "1",
            Unit: null,
            Category: "Pantry"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("100"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyQuantity_ShouldFail(string? quantity)
    {
        // Arrange
        var command = new AddCustomItemCommand(
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            Name: "Item",
            Quantity: quantity!,
            Unit: null,
            Category: "Pantry"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyCategory_ShouldFail(string? category)
    {
        // Arrange
        var command = new AddCustomItemCommand(
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            Name: "Item",
            Quantity: "1",
            Unit: null,
            Category: category!
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Category");
    }

    [Fact]
    public async Task Validate_WithInvalidCategory_ShouldFail()
    {
        // Arrange
        var command = new AddCustomItemCommand(
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            Name: "Item",
            Quantity: "1",
            Unit: null,
            Category: "InvalidCategory"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Category");
    }

    [Theory]
    [InlineData("Produce")]
    [InlineData("Dairy")]
    [InlineData("Meat")]
    [InlineData("Pantry")]
    [InlineData("produce")]
    [InlineData("DAIRY")]
    public async Task Validate_WithValidCategory_ShouldPass(string category)
    {
        // Arrange
        var command = new AddCustomItemCommand(
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            Name: "Item",
            Quantity: "1",
            Unit: null,
            Category: category
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
