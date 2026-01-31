using FluentAssertions;
using MealPlanner.Application.Common.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MealPlanner.Application.Tests.Common.Behaviors;

public sealed class PipelineBehaviorTests
{
    [Fact]
    public async Task Send_WithBehaviors_ShouldExecuteBehaviorsInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(executionOrder);
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>, FirstBehavior>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>, SecondBehavior>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestRequest("input"));

        // Assert
        executionOrder.Should().BeEquivalentTo(
            ["First:Before", "Second:Before", "Handler", "Second:After", "First:After"],
            options => options.WithStrictOrdering());
        result.Should().Be("Handled: input");
    }

    [Fact]
    public async Task Send_WithNoBehaviors_ShouldExecuteHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestRequest("test"));

        // Assert
        result.Should().Be("Handled: test");
    }

    [Fact]
    public async Task Send_WithBehaviorThatThrows_ShouldPropagateException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>, ThrowingBehavior>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var act = () => mediator.Send(new TestRequest("test"));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Behavior error");
    }

    [Fact]
    public async Task Send_WithBehaviorThatShortCircuits_ShouldNotExecuteHandler()
    {
        // Arrange
        var executionOrder = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(executionOrder);
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>, ShortCircuitBehavior>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestRequest("test"));

        // Assert
        result.Should().Be("Short-circuited");
        executionOrder.Should().NotContain("Handler");
    }

    #region Test Types

    private sealed record TestRequest(string Value) : IRequest<string>;

    private sealed class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        private readonly List<string>? _executionOrder;

        public TestRequestHandler(List<string>? executionOrder = null)
        {
            _executionOrder = executionOrder;
        }

        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
        {
            _executionOrder?.Add("Handler");
            return Task.FromResult($"Handled: {request.Value}");
        }
    }

    private sealed class FirstBehavior : IPipelineBehavior<TestRequest, string>
    {
        private readonly List<string> _executionOrder;

        public FirstBehavior(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async Task<string> Handle(TestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            _executionOrder.Add("First:Before");
            var result = await next();
            _executionOrder.Add("First:After");
            return result;
        }
    }

    private sealed class SecondBehavior : IPipelineBehavior<TestRequest, string>
    {
        private readonly List<string> _executionOrder;

        public SecondBehavior(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async Task<string> Handle(TestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            _executionOrder.Add("Second:Before");
            var result = await next();
            _executionOrder.Add("Second:After");
            return result;
        }
    }

    private sealed class ThrowingBehavior : IPipelineBehavior<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Behavior error");
        }
    }

    private sealed class ShortCircuitBehavior : IPipelineBehavior<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            return Task.FromResult("Short-circuited");
        }
    }

    #endregion
}
