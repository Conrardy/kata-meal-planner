using ErrorOr;
using MealPlanner.Application.Common.Mediator;

namespace MealPlanner.Application.Admin;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ErrorOr<CreateUserResponse>>
{
    private readonly IAdminUserService _adminUserService;

    public CreateUserCommandHandler(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    public async Task<ErrorOr<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return await _adminUserService.CreateUserAsync(request.Username, request.Password, cancellationToken);
    }
}
