using ConfigEditor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConfigEditor;

public static class IO
{
    public static async Task SaveToFile(GeneralSettings settings, IEnumerable<Group> groups, Stream stream)
    {
        var masterDictionary = new Dictionary<string, object>
        {
            { "general", settings }
        };

        var groupsDictionary = new Dictionary<string, object>();

        foreach (var group in groups)
        {
            var subDictionary = group.SubGroups.Where(sub => sub.Channels.Count > 0)
                .ToDictionary(sub => sub.Name, sub => sub.Channels.ToArray());

            if (subDictionary.Count > 0) groupsDictionary.Add(group.Name, subDictionary);
        }

        masterDictionary.Add("groups", groupsDictionary);

        await JsonSerializer.SerializeAsync(stream, masterDictionary);
    }

    public static async Task<(GeneralSettings settings, ObservableCollection<Group>)> ReadFromFile(Stream stream)
    {
        var file = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(stream);
        var settings = JsonSerializer.Deserialize<GeneralSettings>(file["general"]);
        var groups = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Channel[]>>>(file["groups"]);

        var groupsCollection = new ObservableCollection<Group>();

        foreach (var pair in groups)
        {
            var group = new Group
            {
                Name = pair.Key,
                SubGroups = new ObservableCollection<SubGroup>()
            };

            foreach (var sub in pair.Value)
            {
                var subGroup = new SubGroup
                {
                    Name = sub.Key,
                    Channels = new ObservableCollection<Channel>(sub.Value)
                };
                group.SubGroups.Add(subGroup);
            }

            groupsCollection.Add(group);
        }

        return (settings, groupsCollection);
    }
}
