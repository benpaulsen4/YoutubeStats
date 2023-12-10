using System.Text.Json;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Spectre.Console;
using YoutubeStats;
using YoutubeStats.Models;
using YoutubeStats.Utilities;

AnsiConsole.MarkupLine("Youtube Stats Collector [green]v2[/] by [bold]Ben Paulsen[/]");

var globalSpinner = AnsiConsole.Status().Spinner(Spinner.Known.Dots).SpinnerStyle(Style.Parse("blue bold"));

const string configLocation = @"config.json";

var readStream = File.OpenRead(configLocation);
var config = await JsonSerializer.DeserializeAsync<Configuration>(readStream);
await readStream.DisposeAsync();

if (config == null) throw new ArgumentException("Config incorrectly configured");
ConfigurationValidator.Validate(config);

var noWait = false;

if (args.Contains("--undo"))
{
    var baseDirectory = Directory.GetCurrentDirectory();
    foreach (var group in config.Groups)
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
    foreach (var group in config.Groups)
    {
        subGroups.Add(group.Key, group.Value.Select(innerSubGroups => innerSubGroups.Key).ToArray());
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

if (channelsWithHandles.Count != 0)
{
    AnsiConsole.MarkupLine(
        "[yellow]:warning: [b]Warning:[/] Channel handles (usernames with @ at the beginning) detected in config file. We will now attempt to convert them to Youtube IDs." +
        " This will also update your config file to replace the handles with IDs, it is recommended that you backup your config file in case of a failure. [/]");
    AnsiConsole.WriteLine();

    if (!AnsiConsole.Confirm("Would you like to continue?")) Environment.Exit(1);

    await globalSpinner.StartAsync("Resolving Youtube handles...", async context =>
    {
        var handleResolver = new LocalHandleResolver();

        await handleResolver.ResolveHandles(channelsWithHandles);

        context.Status("Updating config...");

        File.Delete(configLocation);
        var writeStream = File.OpenWrite(configLocation);
        await JsonSerializer.SerializeAsync(writeStream, config);
        await writeStream.DisposeAsync();
    });
}

var service = new YouTubeService(new BaseClientService.Initializer
{
    ApplicationName = "Youtube Stats",
    ApiKey = config.General.ApiKey
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

    var reportType = config.General.ReportType;

    var reporting = new ReportingService(channels.ToArray(), subGroups, context);

    switch (reportType)
    {
        case ReportType.Console:
            reporting.GenerateConsoleReport();
            break;
        case ReportType.Csv:
            reporting.GenerateCsvReport();
            break;
        case ReportType.Analytics:
#if !DEBUG
            try
            {
                reporting.GenerateAnalyticsReport(false);
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]Error[/] generating analytics report, falling back to console! (CSV may or may not have been written)");
                reporting.GenerateConsoleReport();
            }
#endif
#if DEBUG
            reporting.GenerateAnalyticsReport(false);
#endif
            break;
        case ReportType.AnalyticsSaved:
            reporting.GenerateAnalyticsReport(true);
            break;
        default:
            throw new InvalidOperationException("Unknown report type");
    }
});

AnsiConsole.WriteLine();

if (!noWait)
{
    AnsiConsole.WriteLine("Press any key to exit...");
    Console.ReadKey();
}