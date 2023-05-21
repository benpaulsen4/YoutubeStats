using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YoutubeStats;

Console.WriteLine("Youtube Stats Collector by Ben Paulsen");
Console.WriteLine("Parsing config...");

JObject config = JObject.Parse(File.ReadAllText(@"config.json"));

var groups = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ChannelSummary[]>>>(config["groups"]?.ToString() ?? throw new ArgumentNullException("Groups", "Config missing groups or incorrectly configured"));
var channels = new List<ChannelSummary>();
var subGroups = new Dictionary<string, string[]>();

if (args.Contains("--undo"))
{
    Console.WriteLine("Erasing...");
    if (groups == null) throw new ArgumentException("Config missing groups or incorrectly configured");
    
    var baseDirectory = Directory.GetCurrentDirectory();
    foreach (var group in groups)
    {
        Directory.SetCurrentDirectory($"{baseDirectory}/Results/{group.Key}");

        foreach (var subGroup in group.Value)
        {
            Csv.EraseLatestLine(subGroup.Key);
        }
    }

    Console.WriteLine("Erased latest entries");
    return;
}

foreach (var group in groups!)
{
    subGroups.Add(group.Key, group.Value.Select(subGroups => subGroups.Key).ToArray());
    foreach (var subGroup in group.Value)
    {
        foreach (var channel in subGroup.Value)
        {
            channel.SubGroup = subGroup.Key;
            channels.Add(channel);
        }
    }
}

var service = new YouTubeService(new BaseClientService.Initializer
{
    ApplicationName = "Youtube Stats",
    ApiKey = config["general"]?["apiKey"]?.ToString() ?? throw new ArgumentNullException("API key", "Config missing API key or incorrectly configured")
});

var parts = new List<string> { "statistics" };
var channelIds = channels.Select(channel => channel.Id);

Console.WriteLine("Querying Youtube API...");

var request = service.Channels.List(new Google.Apis.Util.Repeatable<string>(parts));
request.Id = new Google.Apis.Util.Repeatable<string>(channelIds);

var result = await request.ExecuteAsync();

Console.WriteLine("Parsing response...");

foreach(var channel in channels) { 
    foreach (var channelSubs in result.Items)
    {
        if (channelSubs.Id == channel.Id)
        {
            channel.SubscriberCount = (int)channelSubs.Statistics.SubscriberCount!;
            break;
        }
    }
}

var reportType = config["general"]?["reportType"]?.ToString() ?? throw new ArgumentNullException("Report type", "Config missing report type or incorrectly configured");

var reporting = new ReportingService(channels.ToArray(), subGroups);

switch (reportType)
{
    case "console":
        reporting.GenerateConsoleReport();
        break;
    case "csv":
        reporting.GenerateCsvReport();
        break;
    case "analytics":
        try
        {
            reporting.GenerateAnalyticsReport();
        }
        catch (Exception)
        {
            Console.WriteLine("Error generating analytics report, falling back to console! (CSV may or may not have been written)");
            reporting.GenerateConsoleReport();
        }
        break;
    default:
        throw new InvalidOperationException("Unknown report type");
};

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();