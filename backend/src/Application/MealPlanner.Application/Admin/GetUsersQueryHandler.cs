using MealPlanner.Application.Common.Mediator;

namespace MealPlanner.Application.Admin;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, GetUsersResponse>
{
    private readonly IAdminUserService _adminUserService;

    public GetUsersQueryHandler(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    public async Task<GetUsersResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _adminUserService.GetAllUsersAsync(cancellationToken);
    }
}
