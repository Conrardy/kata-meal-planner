using MealPlanner.Application.Auth;
using MealPlanner.Domain.Auth;
using MealPlanner.Infrastructure.Identity;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Auth;

public sealed class UserAuthenticationService : IUserAuthenticationService
{
    private readonly ISeededUserRepository _seededUserRepository;
    private readonly MealPlannerDbContext _dbContext;
    private readonly IPasswordHasher<DynamicUser> _passwordHasher;

    public UserAuthenticationService(
        ISeededUserRepository seededUserRepository,
        MealPlannerDbContext dbContext,
        IPasswordHasher<DynamicUser> passwordHasher)
    {
        _seededUserRepository = seededUserRepository;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticatedUser?> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var seededUser = _seededUserRepository.FindByUsername(username);
        if (seededUser is not null)
        {
            var isValidPassword = _seededUserRepository.ValidatePassword(username, password);
            if (isValidPassword)
            {
                return new AuthenticatedUser(seededUser.Id, seededUser.Username.Value, seededUser.IsAdmin);
            }
            return null;
        }

        var dynamicUser = await _dbContext.DynamicUsers
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);

        if (dynamicUser is null)
        {
            return null;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            dynamicUser, dynamicUser.PasswordHash, password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return new AuthenticatedUser(dynamicUser.Id, dynamicUser.Username, false);
    }

    public async Task<AuthenticatedUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var seededUser = _seededUserRepository.GetAllUsers().FirstOrDefault(u => u.Id == userId);
        if (seededUser is not null)
        {
            return new AuthenticatedUser(seededUser.Id, seededUser.Username.Value, seededUser.IsAdmin);
        }

        var dynamicUser = await _dbContext.DynamicUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (dynamicUser is not null)
        {
            return new AuthenticatedUser(dynamicUser.Id, dynamicUser.Username, false);
        }

        return null;
    }
}
