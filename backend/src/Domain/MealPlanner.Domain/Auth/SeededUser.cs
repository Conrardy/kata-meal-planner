namespace MealPlanner.Domain.Auth;

public sealed record SeededUser
{
    public Guid Id { get; }
    public Username Username { get; }
    public bool IsAdmin { get; }

    private SeededUser(Guid id, Username username, bool isAdmin)
    {
        Id = id;
        Username = username;
        IsAdmin = isAdmin;
    }

    public static SeededUser Create(Guid id, string username, bool isAdmin = false)
    {
        return new SeededUser(id, Username.Create(username), isAdmin);
    }
}
