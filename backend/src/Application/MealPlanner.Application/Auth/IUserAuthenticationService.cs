using MealPlanner.Domain.Auth;

namespace MealPlanner.Application.Auth;

public record AuthenticatedUser(Guid Id, string Username, bool IsAdmin);

public interface IUserAuthenticationService
{
    Task<AuthenticatedUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<AuthenticatedUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
