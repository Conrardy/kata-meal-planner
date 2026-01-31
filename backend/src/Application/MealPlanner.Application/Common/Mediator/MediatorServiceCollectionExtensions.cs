using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Application.Common.Mediator;

/// <summary>
/// Extension methods for registering mediator services with the DI container.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the mediator and all handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided for handler scanning.", nameof(assemblies));
        }

        services.AddScoped<IMediator, Mediator>();

        foreach (var assembly in assemblies)
        {
            RegisterRequestHandlers(services, assembly);
            RegisterNotificationHandlers(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers the mediator and all handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type from the assembly to scan.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediator<T>(this IServiceCollection services)
    {
        return services.AddMediator(typeof(T).Assembly);
    }

    /// <summary>
    /// Registers a pipeline behavior. Behaviors are executed in the order they are registered.
    /// </summary>
    /// <typeparam name="TBehavior">The behavior implementation type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Pipeline execution order:
    /// <code>
    /// Request → First Registered Behavior → Second Registered Behavior → ... → Handler → Response
    /// </code>
    /// Register behaviors in the order you want them to execute (e.g., logging first, then validation, then transaction).
    /// </remarks>
    public static IServiceCollection AddMediatorBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class
    {
        var behaviorType = typeof(TBehavior);

        if (!behaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"Behavior {behaviorType.Name} must be an open generic type (e.g., typeof(ValidationBehavior<,>)).",
                nameof(TBehavior));
        }

        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);

        return services;
    }

    /// <summary>
    /// Registers a pipeline behavior with explicit open generic type.
    /// Behaviors are executed in the order they are registered.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="behaviorType">The open generic behavior type.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediatorBehavior(this IServiceCollection services, Type behaviorType)
    {
        ArgumentNullException.ThrowIfNull(behaviorType);

        if (!behaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"Behavior {behaviorType.Name} must be an open generic type (e.g., typeof(ValidationBehavior<,>)).",
                nameof(behaviorType));
        }

        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);

        return services;
    }

    private static void RegisterRequestHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaceType = typeof(IRequestHandler<,>);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(i => new { Implementation = t, Interface = i }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaceType = typeof(INotificationHandler<>);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(i => new { Implementation = t, Interface = i }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }
    }
}
