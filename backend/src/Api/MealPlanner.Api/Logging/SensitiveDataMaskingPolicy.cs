using Serilog.Core;
using Serilog.Events;

namespace MealPlanner.Api.Logging;

public sealed class SensitiveDataMaskingPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> SensitivePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "Token",
        "AccessToken",
        "RefreshToken",
        "Secret",
        "ApiKey",
        "Authorization",
        "Bearer",
        "Credential",
        "PrivateKey"
    };

    private const string RedactedValue = "***REDACTED***";

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        result = null;

        if (value is null)
            return false;

        var type = value.GetType();

        if (!ShouldDestructure(type))
            return false;

        var properties = type.GetProperties()
            .Where(p => p.CanRead)
            .Select(p =>
            {
                var propertyValue = IsSensitiveProperty(p.Name)
                    ? new ScalarValue(RedactedValue)
                    : propertyValueFactory.CreatePropertyValue(p.GetValue(value), destructureObjects: true);

                return new LogEventProperty(p.Name, propertyValue);
            })
            .ToList();

        result = new StructureValue(properties, type.Name);
        return true;
    }

    private static bool ShouldDestructure(Type type)
    {
        var typeName = type.FullName ?? type.Name;
        return typeName.StartsWith("MealPlanner.", StringComparison.Ordinal);
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        return SensitivePropertyNames.Any(sensitive =>
            propertyName.Contains(sensitive, StringComparison.OrdinalIgnoreCase));
    }
}
