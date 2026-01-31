using ErrorOr;
using MediatR;

namespace MealPlanner.Application.Auth.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<ErrorOr<LoginResponse>>;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Username
);
