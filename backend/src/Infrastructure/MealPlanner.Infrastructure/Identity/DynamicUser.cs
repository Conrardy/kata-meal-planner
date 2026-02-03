namespace MealPlanner.Infrastructure.Identity;

public sealed class DynamicUser
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private DynamicUser() { }

    public static DynamicUser Create(Guid id, string username, string passwordHash)
    {
        return new DynamicUser
        {
            Id = id,
            Username = username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }
}
