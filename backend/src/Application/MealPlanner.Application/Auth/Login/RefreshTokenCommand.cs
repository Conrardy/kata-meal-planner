using ErrorOr;
using MediatR;

namespace MealPlanner.Application.Auth.Login;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<LoginResponse>>;
