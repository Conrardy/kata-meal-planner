using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MealPlanner.Application.Common.Behaviors;
using MealPlanner.Application.Common.Mediator;
using Xunit;
using ValidationException = MealPlanner.Application.Common.Exceptions.ValidationException;

namespace MealPlanner.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommand>>();
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var handlerCalled = false;

        RequestHandlerDelegate<string> next = () =>
        {
            handlerCalled = true;
            return Task.FromResult("success");
        };

        // Act
        var result = await behavior.Handle(new TestCommand("test"), next, CancellationToken.None);

        // Assert
        handlerCalled.Should().BeTrue();
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WithPassingValidator_ShouldCallNext()
    {
        // Arrange
        var validator = new PassingValidator();
        var behavior = new ValidationBehavior<TestCommand, string>(new[] { validator });
        var handlerCalled = false;

        RequestHandlerDelegate<string> next = () =>
        {
            handlerCalled = true;
            return Task.FromResult("success");
        };

        // Act
        var result = await behavior.Handle(new TestCommand("valid"), next, CancellationToken.None);

        // Assert
        handlerCalled.Should().BeTrue();
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new FailingValidator();
        var behavior = new ValidationBehavior<TestCommand, string>(new[] { validator });

        RequestHandlerDelegate<string> next = () => Task.FromResult("should not reach");

        // Act
        var act = () => behavior.Handle(new TestCommand("invalid"), next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Value");
        exception.Which.Errors["Value"].Should().Contain("Value is required");
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldRunAll()
    {
        // Arrange
        var validator1 = new FirstValidator();
        var validator2 = new SecondValidator();
        var behavior = new ValidationBehavior<TestCommand, string>(new IValidator<TestCommand>[] { validator1, validator2 });

        RequestHandlerDelegate<string> next = () => Task.FromResult("should not reach");

        // Act
        var act = () => behavior.Handle(new TestCommand(""), next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Value");
        exception.Which.Errors["Value"].Should().Contain("First error");
        exception.Which.Errors["Value"].Should().Contain("Second error");
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ShouldNotCallNext()
    {
        // Arrange
        var validator = new FailingValidator();
        var behavior = new ValidationBehavior<TestCommand, string>(new[] { validator });
        var handlerCalled = false;

        RequestHandlerDelegate<string> next = () =>
        {
            handlerCalled = true;
            return Task.FromResult("success");
        };

        // Act
        try
        {
            await behavior.Handle(new TestCommand("invalid"), next, CancellationToken.None);
        }
        catch (ValidationException)
        {
            // Expected
        }

        // Assert
        handlerCalled.Should().BeFalse();
    }

    #region Test Types

    private sealed record TestCommand(string Value) : IRequest<string>;

    private sealed class PassingValidator : AbstractValidator<TestCommand>
    {
        public PassingValidator()
        {
            // No rules - always passes
        }
    }

    private sealed class FailingValidator : AbstractValidator<TestCommand>
    {
        public FailingValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<TestCommand> context, CancellationToken cancellation = default)
        {
            return Task.FromResult(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Value is required")
            }));
        }
    }

    private sealed class FirstValidator : AbstractValidator<TestCommand>
    {
        public override Task<ValidationResult> ValidateAsync(ValidationContext<TestCommand> context, CancellationToken cancellation = default)
        {
            return Task.FromResult(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "First error")
            }));
        }
    }

    private sealed class SecondValidator : AbstractValidator<TestCommand>
    {
        public override Task<ValidationResult> ValidateAsync(ValidationContext<TestCommand> context, CancellationToken cancellation = default)
        {
            return Task.FromResult(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Second error")
            }));
        }
    }

    #endregion
}
