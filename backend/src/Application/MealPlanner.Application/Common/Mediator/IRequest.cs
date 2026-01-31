namespace MealPlanner.Application.Common.Mediator;

/// <summary>
/// Marker interface for requests (commands and queries) that return a response.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request handler.</typeparam>
public interface IRequest<out TResponse>;
