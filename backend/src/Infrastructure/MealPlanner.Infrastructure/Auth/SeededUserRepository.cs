using MealPlanner.Domain.Auth;
using Microsoft.Extensions.Options;

namespace MealPlanner.Infrastructure.Auth;

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

public sealed class SeededUserRepository : ISeededUserRepository
{
    private readonly Dictionary<string, SeededUser> _usersByUsername;
    private readonly string _password;

    public SeededUserRepository(IOptions<SeededUsersSettings> settings)
    {
        var config = settings.Value;
        _password = config.Password;

        _usersByUsername = config.Users
            .Where(u => !string.IsNullOrWhiteSpace(u.Username))
            .ToDictionary(
                u => u.Username.ToLowerInvariant(),
                u => SeededUser.Create(GenerateDeterministicGuid(u.Username), u.Username),
                StringComparer.OrdinalIgnoreCase);
    }

    public SeededUser? FindByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return _usersByUsername.TryGetValue(username.ToLowerInvariant(), out var user) ? user : null;
    }

    public bool ValidatePassword(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var user = FindByUsername(username);
        if (user is null)
        {
            return false;
        }

        return string.Equals(_password, password, StringComparison.Ordinal);
    }

    public IReadOnlyList<SeededUser> GetAllUsers()
    {
        return _usersByUsername.Values.ToList().AsReadOnly();
    }

    private static Guid GenerateDeterministicGuid(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input.ToLowerInvariant()));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }
}
