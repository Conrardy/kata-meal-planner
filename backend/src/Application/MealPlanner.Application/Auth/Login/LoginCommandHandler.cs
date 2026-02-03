using ErrorOr;
using MealPlanner.Application.Common.Mediator;

namespace MealPlanner.Application.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    private readonly IUserAuthenticationService _userAuthService;
    private readonly ISeededUserAuthService _tokenService;

    public LoginCommandHandler(
        IUserAuthenticationService userAuthService,
        ISeededUserAuthService tokenService)
    {
        _userAuthService = userAuthService;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authenticatedUser = await _userAuthService.AuthenticateAsync(
            request.Username, request.Password, cancellationToken);

        if (authenticatedUser is null)
        {
            return AuthErrors.InvalidCredentials;
        }

        var result = await _tokenService.GenerateTokensForUserAsync(authenticatedUser, cancellationToken);
        return result;
    }
}
