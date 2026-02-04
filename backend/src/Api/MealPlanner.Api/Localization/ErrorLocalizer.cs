using Microsoft.Extensions.Localization;

namespace MealPlanner.Api.Localization;

public sealed class ErrorLocalizer
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ErrorLocalizer(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string Localize(string errorCode, string fallbackMessage, params object[] args)
    {
        var localized = _localizer[errorCode];
        if (localized.ResourceNotFound)
            return fallbackMessage;

        return args.Length > 0
            ? string.Format(localized.Value, args)
            : localized.Value;
    }

    public string LocalizeByKey(string resourceKey)
    {
        var localized = _localizer[resourceKey];
        return localized.ResourceNotFound ? resourceKey : localized.Value;
    }

    public string LocalizeByKey(string resourceKey, params object[] args)
    {
        var localized = _localizer[resourceKey];
        if (localized.ResourceNotFound)
            return resourceKey;

        return args.Length > 0
            ? string.Format(localized.Value, args)
            : localized.Value;
    }
}
