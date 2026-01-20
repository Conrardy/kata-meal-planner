using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public static ApplicationUser Create(string email)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            CreatedAt = DateTime.UtcNow
        };
    }
}
