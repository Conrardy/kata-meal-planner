using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using MealPlanner.Application.Auth;
using MealPlanner.Application.Auth.Login;
using MealPlanner.Domain.Auth;
using MealPlanner.Infrastructure.Identity;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MealPlanner.Infrastructure.Auth;

public sealed class SeededUserAuthService : ISeededUserAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly MealPlannerDbContext _dbContext;
    private readonly ISeededUserRepository _seededUserRepository;
    private readonly byte[] _keyBytes;

    public SeededUserAuthService(
        IOptions<JwtSettings> jwtSettings,
        MealPlannerDbContext dbContext,
        ISeededUserRepository seededUserRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _dbContext = dbContext;
        _seededUserRepository = seededUserRepository;
        _keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
    }

    public async Task<ErrorOr<LoginResponse>> GenerateTokensAsync(SeededUser user, CancellationToken cancellationToken = default)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshTokenValue = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.Add(TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays));

        var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue, TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays));

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            refreshTokenExpiration,
            user.Id,
            user.Username.Value);
    }

    public async Task<ErrorOr<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        var user = _seededUserRepository.GetAllUsers()
            .FirstOrDefault(u => u.Id == storedToken.UserId);

        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        storedToken.Revoke();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GenerateTokensAsync(user, cancellationToken);
    }

    private string GenerateAccessToken(SeededUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.Username.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(_keyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
