namespace MealPlanner.Infrastructure.Identity;

public sealed class UserIdentityMap
{
    public Guid AspNetUserId { get; private set; }
    public string FirebaseUid { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private UserIdentityMap() { }

    public static UserIdentityMap Create(Guid aspNetUserId, string firebaseUid)
    {
        return new UserIdentityMap
        {
            AspNetUserId = aspNetUserId,
            FirebaseUid = firebaseUid,
            CreatedAt = DateTime.UtcNow
        };
    }
}
