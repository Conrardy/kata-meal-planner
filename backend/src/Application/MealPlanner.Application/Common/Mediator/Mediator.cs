using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Application.Common.Mediator;

/// <summary>
/// Default implementation of the mediator pattern.
/// Resolves handlers from the DI container and dispatches requests/notifications.
/// Supports pipeline behaviors that execute before and after the handler.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No handler registered for request type {requestType.Name}. " +
                $"Ensure a handler implementing IRequestHandler<{requestType.Name}, {typeof(TResponse).Name}> is registered.");

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler type {handlerType.Name}");

        // Create the final handler delegate
        RequestHandlerDelegate<TResponse> handlerDelegate = async () =>
        {
            var result = handleMethod.Invoke(handler, [request, cancellationToken]);
            if (result is not Task<TResponse> task)
            {
                throw new InvalidOperationException(
                    $"Handler for {requestType.Name} did not return Task<{typeof(TResponse).Name}>.");
            }
            return await task;
        };

        // Get pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviorsEnumerableType = typeof(IEnumerable<>).MakeGenericType(behaviorType);
        var behaviors = _serviceProvider.GetService(behaviorsEnumerableType) as IEnumerable<object>;

        if (behaviors is null || !behaviors.Any())
        {
            return await handlerDelegate();
        }

        // Build the pipeline by wrapping behaviors around the handler
        // Behaviors execute in registration order (first registered = outermost)
        var pipeline = behaviors.Reverse().Aggregate(
            handlerDelegate,
            (next, behavior) =>
            {
                var behaviorHandleMethod = behaviorType.GetMethod("Handle")
                    ?? throw new InvalidOperationException($"Handle method not found on behavior type {behaviorType.Name}");

                return async () =>
                {
                    try
                    {
                        var result = behaviorHandleMethod.Invoke(behavior, [request, next, cancellationToken]);
                        if (result is not Task<TResponse> task)
                        {
                            throw new InvalidOperationException(
                                $"Behavior did not return Task<{typeof(TResponse).Name}>.");
                        }
                        return await task;
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException is not null)
                    {
                        throw ex.InnerException;
                    }
                };
            });

        return await pipeline();
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlersEnumerableType = typeof(IEnumerable<>).MakeGenericType(handlerType);

        var handlers = _serviceProvider.GetService(handlersEnumerableType) as IEnumerable<object>;

        if (handlers is null)
        {
            return;
        }

        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on handler type {handlerType.Name}");

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            var result = handleMethod.Invoke(handler, [notification, cancellationToken]);
            if (result is Task task)
            {
                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
    }
}
