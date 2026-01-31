using ErrorOr;
using MealPlanner.Domain.Auth;
using MediatR;

namespace MealPlanner.Application.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    private readonly ISeededUserRepository _seededUserRepository;
    private readonly ISeededUserAuthService _authService;

    public LoginCommandHandler(
        ISeededUserRepository seededUserRepository,
        ISeededUserAuthService authService)
    {
        _seededUserRepository = seededUserRepository;
        _authService = authService;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = _seededUserRepository.FindByUsername(request.Username);
        if (user is null)
        {
            return AuthErrors.InvalidCredentials;
        }

        var isValidPassword = _seededUserRepository.ValidatePassword(request.Username, request.Password);
        if (!isValidPassword)
        {
            return AuthErrors.InvalidCredentials;
        }

        var result = await _authService.GenerateTokensAsync(user, cancellationToken);
        return result;
    }
}
