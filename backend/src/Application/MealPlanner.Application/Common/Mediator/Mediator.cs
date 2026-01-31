using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Application.Common.Mediator;

/// <summary>
/// Default implementation of the mediator pattern.
/// Resolves handlers from the DI container and dispatches requests/notifications.
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

        var result = handleMethod.Invoke(handler, [request, cancellationToken]);

        if (result is not Task<TResponse> task)
        {
            throw new InvalidOperationException(
                $"Handler for {requestType.Name} did not return Task<{typeof(TResponse).Name}>.");
        }

        return await task;
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
