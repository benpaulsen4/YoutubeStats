using YoutubeStats.Models;

namespace YoutubeStats.Utilities;

public static class ConfigurationValidator
{
    public static void Validate(Configuration config)
    {
        ThrowIfNull(config.General, "General Settings");
        ThrowIfNull(config.General?.ApiKey, "API Key");
        ThrowIfNull(config.General?.ReportType, "Report Type");
        ThrowIfNull(config.Groups, "Groups");

        foreach (var group in config.Groups)
        {
            ThrowIfNull(group.Value, $"{group.Key} Sub-Groups");

            foreach (var subGroup in group.Value)
            {
                ThrowIfNull(subGroup.Value, $"{subGroup.Key} Channels");

                foreach (var channel in subGroup.Value)
                {
                    ThrowIfNull(channel.Id, $"Channel ID (a channel in {subGroup.Key})");
                    ThrowIfNull(channel.Name, $"Channel Name (a channel in {subGroup.Key})");
                }
            }
        }
    }

    private static void ThrowIfNull<T>(T? obj, string name)
    {
        if (obj == null) throw new ArgumentNullException(name, "This value cannot be null");
    }
}