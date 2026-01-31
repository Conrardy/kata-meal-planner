namespace MealPlanner.Application.Common.Mediator;

/// <summary>
/// Marker interface for requests that should be wrapped in a database transaction.
/// Apply this interface to commands that modify data and require transactional consistency.
/// </summary>
/// <remarks>
/// When a request implements this interface, the TransactionBehavior will:
/// 1. Begin a transaction before executing the handler
/// 2. Commit the transaction if the handler completes successfully
/// 3. Rollback the transaction if the handler throws an exception
/// </remarks>
public interface ITransactionalRequest;
