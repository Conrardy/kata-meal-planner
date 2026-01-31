using FluentAssertions;
using MealPlanner.Application.Common.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MealPlanner.Application.Tests.Common.Mediator;

public sealed class MediatorTests
{
    [Fact]
    public async Task Send_WithRegisteredHandler_ShouldReturnResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestQuery, TestResponse>, TestQueryHandler>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
        var query = new TestQuery("Hello");

        // Act
        var result = await mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Response: Hello");
    }

    [Fact]
    public async Task Send_WithNoRegisteredHandler_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
        var query = new UnhandledQuery();

        // Act
        var act = () => mediator.Send(query);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered*UnhandledQuery*");
    }

    [Fact]
    public async Task Send_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var act = () => mediator.Send<string>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Send_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<CancellableQuery, bool>, CancellableQueryHandler>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await mediator.Send(new CancellableQuery(), cts.Token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Publish_WithMultipleHandlers_ShouldNotifyAllHandlers()
    {
        // Arrange
        var counter = new NotificationCounter();
        var services = new ServiceCollection();
        services.AddSingleton(counter);
        services.AddScoped<INotificationHandler<TestNotification>, FirstNotificationHandler>();
        services.AddScoped<INotificationHandler<TestNotification>, SecondNotificationHandler>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
        var notification = new TestNotification("Test");

        // Act
        await mediator.Publish(notification);

        // Assert
        counter.Count.Should().Be(2);
    }

    [Fact]
    public async Task Publish_WithNoHandlers_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
        var notification = new UnhandledNotification();

        // Act
        var act = () => mediator.Publish(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Publish_WithNullNotification_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var act = () => mediator.Publish(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Send_WithScopedHandler_ShouldResolvePerScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<CounterQuery, int>, CounterQueryHandler>();
        services.AddScoped<IMediator, MealPlanner.Application.Common.Mediator.Mediator>();
        var provider = services.BuildServiceProvider();

        int result1, result2;
        using (var scope1 = provider.CreateScope())
        {
            var mediator = scope1.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new CounterQuery());
            result1 = await mediator.Send(new CounterQuery());
        }

        using (var scope2 = provider.CreateScope())
        {
            var mediator = scope2.ServiceProvider.GetRequiredService<IMediator>();
            result2 = await mediator.Send(new CounterQuery());
        }

        // Assert
        result1.Should().Be(2);
        result2.Should().Be(1);
    }

    #region Test Types

    private sealed record TestQuery(string Input) : IRequest<TestResponse>;
    private sealed record TestResponse(string Message);

    private sealed class TestQueryHandler : IRequestHandler<TestQuery, TestResponse>
    {
        public Task<TestResponse> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResponse($"Response: {request.Input}"));
        }
    }

    private sealed record UnhandledQuery : IRequest<string>;

    private sealed record CancellableQuery : IRequest<bool>;

    private sealed class CancellableQueryHandler : IRequestHandler<CancellableQuery, bool>
    {
        public Task<bool> Handle(CancellableQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(cancellationToken.IsCancellationRequested);
        }
    }

    private sealed record CounterQuery : IRequest<int>;

    private sealed class CounterQueryHandler : IRequestHandler<CounterQuery, int>
    {
        private int _count;

        public Task<int> Handle(CounterQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(++_count);
        }
    }

    private sealed record TestNotification(string Message) : INotification;
    private sealed record UnhandledNotification : INotification;

    private sealed class NotificationCounter
    {
        public int Count { get; set; }
    }

    private sealed class FirstNotificationHandler : INotificationHandler<TestNotification>
    {
        private readonly NotificationCounter _counter;

        public FirstNotificationHandler(NotificationCounter counter)
        {
            _counter = counter;
        }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            _counter.Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class SecondNotificationHandler : INotificationHandler<TestNotification>
    {
        private readonly NotificationCounter _counter;

        public SecondNotificationHandler(NotificationCounter counter)
        {
            _counter = counter;
        }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            _counter.Count++;
            return Task.CompletedTask;
        }
    }

    #endregion
}
