#nullable enable
using System.Text.Json.Serialization;

namespace ConfigEditor.Models;

public record GeneralSettings
{
  [JsonPropertyName("apiKey")] public string? ApiKey { get; set; }
  [JsonPropertyName("reportType")] public string? ReportType { get; set; }
}