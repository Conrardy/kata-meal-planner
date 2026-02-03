using ErrorOr;
using MealPlanner.Application.Admin;
using MealPlanner.Domain.Auth;
using MealPlanner.Infrastructure.Identity;
using MealPlanner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Admin;

public sealed class AdminUserService : IAdminUserService
{
    private readonly MealPlannerDbContext _dbContext;
    private readonly ISeededUserRepository _seededUserRepository;
    private readonly IPasswordHasher<DynamicUser> _passwordHasher;
    private readonly IPasswordValidator<ApplicationUser> _passwordValidator;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(
        MealPlannerDbContext dbContext,
        ISeededUserRepository seededUserRepository,
        IPasswordHasher<DynamicUser> passwordHasher,
        IPasswordValidator<ApplicationUser> passwordValidator,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _seededUserRepository = seededUserRepository;
        _passwordHasher = passwordHasher;
        _passwordValidator = passwordValidator;
        _userManager = userManager;
    }

    public async Task<ErrorOr<CreateUserResponse>> CreateUserAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var seededUser = _seededUserRepository.FindByUsername(username);
        if (seededUser is not null)
        {
            return AdminErrors.UsernameAlreadyExists;
        }

        var existingDynamicUser = await _dbContext.DynamicUsers
            .AnyAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);
        if (existingDynamicUser)
        {
            return AdminErrors.UsernameAlreadyExists;
        }

        var tempUser = ApplicationUser.Create($"{username}@temp.local");
        var passwordValidationResult = await _passwordValidator.ValidateAsync(_userManager, tempUser, password);
        if (!passwordValidationResult.Succeeded)
        {
            var errors = passwordValidationResult.Errors.Select(e => e.Description);
            return AdminErrors.PasswordRequirementsNotMet(errors);
        }

        var userId = Guid.NewGuid();
        var passwordHash = _passwordHasher.HashPassword(null!, password);
        var dynamicUser = DynamicUser.Create(userId, username, passwordHash);

        _dbContext.DynamicUsers.Add(dynamicUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse(userId, username);
    }

    public async Task<GetUsersResponse> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var seededUsers = _seededUserRepository.GetAllUsers()
            .Select(u => new UserDto(u.Id, u.Username.Value, u.IsAdmin))
            .ToList();

        var dynamicUsers = await _dbContext.DynamicUsers
            .Select(u => new UserDto(u.Id, u.Username, false))
            .ToListAsync(cancellationToken);

        var allUsers = seededUsers.Concat(dynamicUsers).ToList();
        return new GetUsersResponse(allUsers);
    }
}
