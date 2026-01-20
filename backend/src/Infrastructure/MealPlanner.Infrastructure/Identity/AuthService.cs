using ErrorOr;
using MealPlanner.Application.Auth;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly MealPlannerDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenProvider jwtTokenProvider,
        MealPlannerDbContext dbContext)
    {
        _userManager = userManager;
        _jwtTokenProvider = jwtTokenProvider;
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return AuthErrors.EmailAlreadyExists;
        }

        var user = ApplicationUser.Create(request.Email);
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return AuthErrors.PasswordRequirementsNotMet(errors);
        }

        return new RegisterResponse(user.Id, user.Email!);
    }

    public async Task<ErrorOr<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthErrors.InvalidCredentials;
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            return AuthErrors.InvalidCredentials;
        }

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<ErrorOr<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        storedToken.Revoke();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenProvider.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenProvider.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            _jwtTokenProvider.GetRefreshTokenValidFor());

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            _jwtTokenProvider.GetAccessTokenExpiration(),
            refreshToken.ExpiresAt,
            user.Id,
            user.Email!);
    }
}
