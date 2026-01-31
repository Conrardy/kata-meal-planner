using FluentAssertions;
using MealPlanner.Application.Common.Behaviors;
using MealPlanner.Application.Common.Interfaces;
using MealPlanner.Application.Common.Mediator;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MealPlanner.Application.Tests.Common.Behaviors;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_NonTransactionalRequest_ShouldNotStartTransaction()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var logger = new FakeLogger<TransactionBehavior<NonTransactionalCommand, string>>();
        var behavior = new TransactionBehavior<NonTransactionalCommand, string>(unitOfWork, logger);

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var result = await behavior.Handle(new NonTransactionalCommand(), next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
        unitOfWork.BeginTransactionCalled.Should().BeFalse();
        unitOfWork.CommitTransactionCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_TransactionalRequest_ShouldStartAndCommitTransaction()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var logger = new FakeLogger<TransactionBehavior<TransactionalCommand, string>>();
        var behavior = new TransactionBehavior<TransactionalCommand, string>(unitOfWork, logger);

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var result = await behavior.Handle(new TransactionalCommand(), next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
        unitOfWork.BeginTransactionCalled.Should().BeTrue();
        unitOfWork.CommitTransactionCalled.Should().BeTrue();
        unitOfWork.RollbackTransactionCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_TransactionalRequest_WhenHandlerThrows_ShouldRollback()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var logger = new FakeLogger<TransactionBehavior<TransactionalCommand, string>>();
        var behavior = new TransactionBehavior<TransactionalCommand, string>(unitOfWork, logger);

        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("Handler error");

        // Act
        var act = () => behavior.Handle(new TransactionalCommand(), next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        unitOfWork.BeginTransactionCalled.Should().BeTrue();
        unitOfWork.CommitTransactionCalled.Should().BeFalse();
        unitOfWork.RollbackTransactionCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TransactionalRequest_WhenAlreadyInTransaction_ShouldSkip()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork { HasActiveTransaction = true };
        var logger = new FakeLogger<TransactionBehavior<TransactionalCommand, string>>();
        var behavior = new TransactionBehavior<TransactionalCommand, string>(unitOfWork, logger);

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        var result = await behavior.Handle(new TransactionalCommand(), next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
        unitOfWork.BeginTransactionCalled.Should().BeFalse();
        unitOfWork.CommitTransactionCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_TransactionalRequest_ShouldLogTransactionLifecycle()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var logger = new FakeLogger<TransactionBehavior<TransactionalCommand, string>>();
        var behavior = new TransactionBehavior<TransactionalCommand, string>(unitOfWork, logger);

        RequestHandlerDelegate<string> next = () => Task.FromResult("result");

        // Act
        await behavior.Handle(new TransactionalCommand(), next, CancellationToken.None);

        // Assert
        logger.LogEntries.Should().HaveCount(2);
        logger.LogEntries[0].Message.Should().Contain("Beginning transaction");
        logger.LogEntries[1].Message.Should().Contain("committed");
    }

    #region Test Types

    private sealed record NonTransactionalCommand : IRequest<string>;

    private sealed record TransactionalCommand : IRequest<string>, ITransactionalRequest;

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool HasActiveTransaction { get; set; }
        public bool BeginTransactionCalled { get; private set; }
        public bool CommitTransactionCalled { get; private set; }
        public bool RollbackTransactionCalled { get; private set; }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginTransactionCalled = true;
            HasActiveTransaction = true;
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            CommitTransactionCalled = true;
            HasActiveTransaction = false;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            RollbackTransactionCalled = true;
            HasActiveTransaction = false;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<LogEntry> LogEntries { get; } = new();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogEntries.Add(new LogEntry
            {
                LogLevel = logLevel,
                Message = formatter(state, exception)
            });
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    public sealed class LogEntry
    {
        public LogLevel LogLevel { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    #endregion
}
