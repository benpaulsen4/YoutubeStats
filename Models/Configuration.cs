using System.Text.Json.Serialization;

namespace YoutubeStats.Models;

public record Configuration
{
    [JsonPropertyName("general")] public GeneralSettings General { get; set; } = null!;

    [JsonPropertyName("groups")]
    public Dictionary<string, Dictionary<string, ChannelSummary[]>> Groups { get; set; } = null!;
}

public record GeneralSettings
{
    [JsonPropertyName("apiKey")] public string ApiKey { get; set; } = null!;

    [JsonPropertyName("reportType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReportType ReportType { get; set; }
}

public enum ReportType
{
    Console,
    Csv,
    Analytics,
    AnalyticsSaved
}