using ErrorOr;
using MealPlanner.Application.Auth.Login;
using MealPlanner.Domain.Auth;

namespace MealPlanner.Application.Auth;

public interface ISeededUserAuthService
{
    Task<ErrorOr<LoginResponse>> GenerateTokensAsync(SeededUser user, CancellationToken cancellationToken = default);
    Task<ErrorOr<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
