namespace MealPlanner.Domain.Auth;

public sealed record Username
{
    public string Value { get; }

    private Username(string value) => Value = value;

    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Username cannot be empty.", nameof(value));
        }

        if (value.Length < 2 || value.Length > 50)
        {
            throw new ArgumentException("Username must be between 2 and 50 characters.", nameof(value));
        }

        return new Username(value.Trim());
    }

    public override string ToString() => Value;
}
