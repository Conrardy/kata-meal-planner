namespace MealPlanner.Domain.Auth;

public sealed record SeededUser
{
    public Guid Id { get; }
    public Username Username { get; }

    private SeededUser(Guid id, Username username)
    {
        Id = id;
        Username = username;
    }

    public static SeededUser Create(Guid id, string username)
    {
        return new SeededUser(id, Username.Create(username));
    }
}
