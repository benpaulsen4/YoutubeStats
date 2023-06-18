using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using YoutubeStats;
using YoutubeStats.Models;
using YoutubeStats.Utilities;

AnsiConsole.MarkupLine("Youtube Stats Collector [green]v2[/] by [bold]Ben Paulsen[/]");

var globalSpinner = AnsiConsole.Status().Spinner(Spinner.Known.Dots).SpinnerStyle(Style.Parse("blue bold"));

const string configLocation = @"config.json";

JObject config = JObject.Parse(File.ReadAllText(configLocation));
var groups = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ChannelSummary[]>>>(config["groups"]?.ToString() ?? throw new ArgumentNullException("Groups", "Config missing groups or incorrectly configured"));
var noWait = false;

if (args.Contains("--undo"))
{
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

    AnsiConsole.MarkupLine("[red]Erased latest entries[/]");
    Environment.Exit(0);
}

if (args.Contains("--no-wait")) noWait = true;

var channels = new List<ChannelSummary>();
var subGroups = new Dictionary<string, string[]>();
var channelsWithHandles = new List<ChannelSummary>();

globalSpinner.Start("Parsing Config...", context =>
{
    foreach (var group in groups!)
    {
        subGroups.Add(group.Key, group.Value.Select(subGroups => subGroups.Key).ToArray());
        foreach (var subGroup in group.Value)
        {
            foreach (var channel in subGroup.Value)
            {
                channel.SubGroup = subGroup.Key;
                channels.Add(channel);

                if (channel.Id[0] == '@') channelsWithHandles.Add(channel);
            }
        }
    }
});

if (channelsWithHandles.Any())
{
    AnsiConsole.MarkupLine("[yellow]:warning: [b]Warning:[/] Channel handles (usernames with @ at the beginning) detected in config file. We will now attempt to convert them to Youtube IDs." + 
        " [u]Please note[/] that this uses an external service not affiliated with Youtube, use at your own risk! \n\nThis will also update your config file to replace the handles with IDs," + 
        " it is recommended that you backup your config file in case of a failure. [/]");
    AnsiConsole.WriteLine();

    if (!AnsiConsole.Confirm("Would you like to continue?")) Environment.Exit(1);

    await globalSpinner.StartAsync("Resolving Youtube handles...", async context =>
    {
        await HandleResolver.ResolveHandles(channelsWithHandles);

        context.Status("Updating config...");

        var configUpdater = new ConfigUpdater(configLocation, config);

        configUpdater.UpdateGroups(groups);
    });
}

var service = new YouTubeService(new BaseClientService.Initializer
{
    ApplicationName = "Youtube Stats",
    ApiKey = config["general"]?["apiKey"]?.ToString() ?? throw new ArgumentNullException("API key", "Config missing API key or incorrectly configured")
});

var parts = new List<string> { "statistics" };
var channelIds = channels.Select(channel => channel.Id);

await globalSpinner.StartAsync("Querying Youtube API...", async context =>
{
    foreach (var chunk in channelIds.Chunk(50))
    {
        var request = service.Channels.List(new Google.Apis.Util.Repeatable<string>(parts));
        request.Id = new Google.Apis.Util.Repeatable<string>(chunk);

        var result = await request.ExecuteAsync();

        foreach (var channel in channels)
        {
            foreach (var channelSubs in result.Items)
            {
                if (channelSubs.Id == channel.Id)
                {
                    channel.SubscriberCount = (int)channelSubs.Statistics.SubscriberCount!;
                    break;
                }
            }
        }
    }

    var reportType = config["general"]?["reportType"]?.ToString() ?? throw new ArgumentNullException("Report type", "Config missing report type or incorrectly configured");

    var reporting = new ReportingService(channels.ToArray(), subGroups, context);

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
                AnsiConsole.MarkupLine("[red]Error[/] generating analytics report, falling back to console! (CSV may or may not have been written)");
                reporting.GenerateConsoleReport();
            }
            break;
        default:
            throw new InvalidOperationException("Unknown report type");
    };
});

AnsiConsole.WriteLine();

if (!noWait)
{
    AnsiConsole.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
    