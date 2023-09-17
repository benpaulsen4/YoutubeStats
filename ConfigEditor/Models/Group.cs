using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    public string Name { get; set; }
    public string Id { get; set; }
    public bool? IgnoreInAverage { get; set; }
}

