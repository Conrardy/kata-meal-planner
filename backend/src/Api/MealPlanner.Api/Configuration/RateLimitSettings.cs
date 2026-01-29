namespace MealPlanner.Api.Configuration;

public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicySettings DefaultPolicy { get; init; } = new();
    public RateLimitPolicySettings AuthPolicy { get; init; } = new();
}

public sealed class RateLimitPolicySettings
{
    public int PermitLimit { get; init; }
    public int WindowInSeconds { get; init; }
    public int QueueLimit { get; init; }
}
