using ErrorOr;

namespace MealPlanner.Application.Admin;

public interface IAdminUserService
{
    Task<ErrorOr<CreateUserResponse>> CreateUserAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<GetUsersResponse> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
