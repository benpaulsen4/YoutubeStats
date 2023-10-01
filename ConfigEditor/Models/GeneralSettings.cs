#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConfigEditor.Models
{
    public record GeneralSettings
    {
        [JsonPropertyName("apiKey")]
        public string? ApiKey { get; set; }
        [JsonPropertyName("reportType")]
        public string? ReportType { get; set; }
    }
}
