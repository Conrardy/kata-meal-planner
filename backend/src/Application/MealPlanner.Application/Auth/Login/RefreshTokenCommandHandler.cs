using ErrorOr;
using MealPlanner.Application.Common.Mediator;

namespace MealPlanner.Application.Auth.Login;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<LoginResponse>>
{
    private readonly ISeededUserAuthService _authService;

    public RefreshTokenCommandHandler(ISeededUserAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
    }
}
