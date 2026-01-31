using FluentAssertions;
using MealPlanner.Application.Common.Behaviors;
using MealPlanner.Application.Common.Mediator;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MealPlanner.Application.Tests.Common.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldLogStartAndCompletion()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestQuery, string>>();
        var behavior = new LoggingBehavior<TestQuery, string>(logger);

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var result = await behavior.Handle(new TestQuery("input"), next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
        logger.LogEntries.Should().HaveCount(2);
        logger.LogEntries[0].LogLevel.Should().Be(LogLevel.Information);
        logger.LogEntries[0].Message.Should().Contain("Handling TestQuery");
        logger.LogEntries[1].LogLevel.Should().Be(LogLevel.Information);
        logger.LogEntries[1].Message.Should().Contain("Handled TestQuery");
        logger.LogEntries[1].Message.Should().Contain("ms");
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldLogError()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestQuery, string>>();
        var behavior = new LoggingBehavior<TestQuery, string>(logger);

        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("Test error");

        // Act
        var act = () => behavior.Handle(new TestQuery("input"), next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        logger.LogEntries.Should().HaveCount(2);
        logger.LogEntries[0].LogLevel.Should().Be(LogLevel.Information);
        logger.LogEntries[1].LogLevel.Should().Be(LogLevel.Error);
        logger.LogEntries[1].Message.Should().Contain("TestQuery");
        logger.LogEntries[1].Message.Should().Contain("failed");
        logger.LogEntries[1].Message.Should().Contain("Test error");
    }

    [Fact]
    public async Task Handle_ShouldPassResultThrough()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestQuery, string>>();
        var behavior = new LoggingBehavior<TestQuery, string>(logger);
        var expectedResult = "expected result";

        RequestHandlerDelegate<string> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(new TestQuery("input"), next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_ShouldLogRequestContent()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestQuery, string>>();
        var behavior = new LoggingBehavior<TestQuery, string>(logger);

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        await behavior.Handle(new TestQuery("test-value"), next, CancellationToken.None);

        // Assert
        logger.LogEntries[0].State.Should().Contain("TestQuery");
    }

    #region Test Types

    private sealed record TestQuery(string Value) : IRequest<string>;

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<LogEntry> LogEntries { get; } = new();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogEntries.Add(new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                State = state?.ToString() ?? string.Empty,
                Exception = exception,
                Message = formatter(state, exception)
            });
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    public sealed class LogEntry
    {
        public LogLevel LogLevel { get; init; }
        public EventId EventId { get; init; }
        public string State { get; init; } = string.Empty;
        public Exception? Exception { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    #endregion
}
