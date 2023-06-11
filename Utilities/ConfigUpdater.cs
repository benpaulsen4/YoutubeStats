using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YoutubeStats.Models;

namespace YoutubeStats.Utilities
{
    internal class ConfigUpdater
    {
        private readonly string FileLocation;
        private readonly JObject ExistingConfig;

        public ConfigUpdater(string fileLocation, JObject existingConfig)
        {
            FileLocation = fileLocation;
            ExistingConfig = existingConfig;
        }

        public void UpdateGroups(Dictionary<string, Dictionary<string, ChannelSummary[]>>? groups)
        {
            var groupString = JsonConvert.SerializeObject(groups, Formatting.Indented);
            
            ExistingConfig["groups"] = JToken.Parse(groupString);

            File.WriteAllText(FileLocation, ExistingConfig.ToString());
        }
    }
}
