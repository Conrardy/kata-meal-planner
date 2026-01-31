namespace MealPlanner.Api.Configuration;

public sealed class SeededUsersSettings
{
    public const string SectionName = "SeededUsers";

    public List<SeededUserEntry> Users { get; init; } = [];
    public string Password { get; init; } = string.Empty;
}

public sealed class SeededUserEntry
{
    public string Username { get; init; } = string.Empty;
}
