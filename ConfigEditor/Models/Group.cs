using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace ConfigEditor.Models;

public record Group
{
    public string Name { get; set; }
    public ObservableCollection<SubGroup> SubGroups { get; set; }
}

public record SubGroup
{
    public string Name { get; set; }
    public ObservableCollection<Channel> Channels { get; set; }
}

public record Channel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("ignoreInAverage")]
    public bool? IgnoreInAverage { get; set; }
}

