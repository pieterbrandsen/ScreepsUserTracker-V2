using System.Collections.Specialized;

namespace UserTrackerShared.Helpers;

internal sealed class AppSettingsReader
{
    private readonly NameValueCollection _settings;

    public AppSettingsReader(NameValueCollection settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public string GetString(string key, string defaultValue = "")
    {
        return _settings[key] ?? defaultValue;
    }

    public string GetRequiredString(string key)
    {
        var value = _settings[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Missing appSetting '{key}'.");
        }

        return value;
    }

    public int GetRequiredInt(string key)
    {
        var value = _settings[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Missing appSetting '{key}'.");
        }

        if (int.TryParse(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid appSetting '{key}' (expected int).");
    }

    public bool GetRequiredBool(string key)
    {
        var value = _settings[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Missing appSetting '{key}'.");
        }

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        if (int.TryParse(value, out var intValue) && (intValue == 0 || intValue == 1))
        {
            return intValue == 1;
        }

        throw new ArgumentException($"Invalid appSetting '{key}' (expected bool).");
    }
}
