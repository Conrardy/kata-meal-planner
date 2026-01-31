using ErrorOr;
using MealPlanner.Application.Common.Mediator;

namespace MealPlanner.Application.Auth.Login;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<LoginResponse>>;
