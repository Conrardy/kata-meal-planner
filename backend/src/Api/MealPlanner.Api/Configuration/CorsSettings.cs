namespace MealPlanner.Api.Configuration;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public string PolicyName { get; init; } = "MealPlannerCorsPolicy";
    public List<string> AllowedOrigins { get; init; } = [];
    public List<string> AllowedHeaders { get; init; } = [];
    public List<string> AllowedMethods { get; init; } = [];
    public bool AllowCredentials { get; init; }
    public int PreflightMaxAgeSeconds { get; init; } = 600;
}
