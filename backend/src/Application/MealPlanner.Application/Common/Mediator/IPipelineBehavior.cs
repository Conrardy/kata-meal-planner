namespace MealPlanner.Application.Common.Mediator;

/// <summary>
/// Delegate representing the next action in the pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <returns>A task representing the asynchronous operation with the response.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Pipeline behavior to surround the inner handler.
/// Implementations add logic to be executed before and/or after the inner handler.
/// Behaviors are executed in the order they are registered.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
/// <remarks>
/// Pipeline Pattern:
/// <code>
/// Request → Behavior1 → Behavior2 → ... → BehaviorN → Handler → Response
///                                                          ↓
///         Response ← Behavior1 ← Behavior2 ← ... ← BehaviorN
/// </code>
/// Each behavior receives the request and a delegate to call the next behavior in the chain.
/// The final delegate invokes the actual handler.
/// </remarks>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request by executing logic before and/or after calling the next delegate.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">A delegate to call the next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation with the response.</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
