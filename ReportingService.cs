using Newtonsoft.Json;
using Spectre.Console;
using YoutubeStats.Models;
using YoutubeStats.Utilities;

namespace YoutubeStats
{
    public class ReportingService
    {
        private readonly ChannelSummary[] data;
        private readonly Dictionary<string, string[]> groupStructure;
        private readonly StatusContext statusContext;

        public ReportingService(ChannelSummary[] data, Dictionary<string, string[]> groupStructure, StatusContext statusContext)
        {
            this.data = data;
            this.groupStructure = groupStructure;
            this.statusContext = statusContext;
        }

        public void GenerateConsoleReport(AnalyticsPackage? package = null)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"Youtube Stats Report - [blue]{DateTime.Now}[/]");
            AnsiConsole.WriteLine();

            ConsoleWriter.WriteReport(data, groupStructure, package);
            if (package != null)
            {
                AnsiConsole.MarkupLine("[b]Notes:[/]");
                AnsiConsole.WriteLine("  * MAG = Monthly Average Growth");
                AnsiConsole.WriteLine("  * Recent MAG is based on the last 3 months of data");
                AnsiConsole.WriteLine("  * The prediction is for 6 months from now");
                AnsiConsole.WriteLine("  * Check the results folder to find graph PNGs of the results");
            }
        }

        public string GenerateCsvReport(bool skipConsole = false)
        {
            statusContext.Status("Saving CSV...");

            PrepareFileStructure();

            var baseDirectory = Directory.GetCurrentDirectory();

            foreach (var group in groupStructure)
            {
                Directory.SetCurrentDirectory($"{baseDirectory}/Results/{group.Key}");

                foreach (var subGroup in group.Value)
                {
                    Csv.Export(subGroup, data.Where(channel => channel.SubGroup == subGroup).ToArray());
                }
            }

            if (!skipConsole)
            {
                GenerateConsoleReport();
            }

            return baseDirectory;
        }

        public void GenerateAnalyticsReport(bool saveReport)
        {
            var baseDirectory = GenerateCsvReport(true);
            var package = new AnalyticsPackage();

            statusContext.Status("Processing analytics...");

            foreach (var group in groupStructure)
            {
                Directory.SetCurrentDirectory($"{baseDirectory}/Results/{group.Key}");

                var subGroupAverages = new Dictionary<DateTime, Dictionary<string, int>>();

                foreach (var subGroup in group.Value)
                {
                    var previousResults = Csv.Read(subGroup);

                    var maxIndex = previousResults.Max(row => row.Index);
                    var latestRow = previousResults.Where(row => row.Index == maxIndex).FirstOrDefault();
                    var previousRow = previousResults.Where(row => row.Index == maxIndex - 1).FirstOrDefault();
                    var firstRow = previousResults.Where(row => row.Index == 0).FirstOrDefault();
                    var threeMonthsAgo = previousResults.Where(row => DateTime.Now.Subtract(row.Date) > TimeSpan.FromDays(90)).LastOrDefault();

                    if (latestRow != null && previousRow != null)
                    {
                        foreach (var channel in latestRow.Values)
                        {
                            foreach (var prevChannel in previousRow.Values)
                            {
                                if (channel.Key == prevChannel.Key)
                                {
                                    double percentChange = ((channel.Value - prevChannel.Value) / (double)prevChannel.Value) * 100;
                                    int exactChange = channel.Value - prevChannel.Value;
                                    package.Change.Add(channel.Key, (exactChange, percentChange));
                                    break;
                                }
                            }

                            foreach (var initialChannel in firstRow!.Values) //if previous row exists so does first row
                            {
                                if (channel.Key == initialChannel.Key)
                                {
                                    double subscriberDelta = channel.Value - initialChannel.Value;
                                    int dayDelta = latestRow.Date.Subtract(firstRow.Date).Days;
                                    double lifetimeMAG = subscriberDelta / dayDelta * 30;
                                    package.LifetimeMonthlyAverageGrowth.Add(channel.Key, (int)lifetimeMAG);
                                    break;
                                }
                            }

                            foreach (var recentChannel in threeMonthsAgo?.Values ?? new Dictionary<string, int>())
                            {
                                if (channel.Key == recentChannel.Key)
                                {
                                    double subscriberDelta = channel.Value - recentChannel.Value;
                                    //Note: Not likely to be exactly three months
                                    int dayDelta = latestRow.Date.Subtract(firstRow.Date).Days;
                                    double recentMAG = subscriberDelta / dayDelta * 30;
                                    package.RecentMonthlyAverageGrowth.Add(channel.Key, (int)recentMAG);
                                    break;
                                }
                            }

                            var MAG = package.RecentMonthlyAverageGrowth.GetNullable(channel.Key) ?? package.LifetimeMonthlyAverageGrowth.GetNullable(channel.Key);
                            if (MAG.HasValue)
                            {
                                int prediction = (MAG.Value * 6) + channel.Value;
                                package.Prediction.Add(channel.Key, prediction);
                            }
                        }
                    }

                    try
                    {
                        ChartGenerators.GenerateSubGroupGraph(previousResults, subGroup);
                    }
                    catch (Exception e)
                    {
                        AnsiConsole.MarkupLine($"[red]Error[/] generating {subGroup} sub-group graph: {e.Message}");
                    }

                    var averageIgnored = data.Where(channel => channel.IgnoreInAverage == true).Select(channel => channel.Name).ToArray();

                    foreach (var row in previousResults)
                    {
                        if (subGroupAverages.TryGetValue(row.Date, out var averages))
                        {
                            averages.Add(subGroup, (int)row.Values.Where(channel => !averageIgnored.Contains(channel.Key)).Select(channel => channel.Value).Average());
                        }
                        else
                        {
                            subGroupAverages.Add(row.Date, new Dictionary<string, int>() { { subGroup, (int)row.Values.Where(channel => !averageIgnored.Contains(channel.Key)).Select(channel => channel.Value).Average() } });
                        }

                    }
                }

                try
                {
                    ChartGenerators.GenerateGroupGraph(subGroupAverages, group.Key);
                }
                catch (Exception e)
                {
                    AnsiConsole.MarkupLine($"[red]Error[/] generating {group.Key} group graph: {e.Message}");
                }
            }
            if (saveReport)
            {
                SaveAnalyticsReport(package, baseDirectory);
            }
            else
            {
                GenerateConsoleReport(package);
            }
        }

        private void SaveAnalyticsReport(AnalyticsPackage analytics, string baseDirectory)
        {
            var channelsWithAnalytics = new List<ChannelWithAnalytics>();
            foreach (var channel in data)
            {
                channelsWithAnalytics.Add(new ChannelWithAnalytics
                {
                    Name = channel.Name,
                    SubGroup = channel.SubGroup,
                    SubscriberCount = channel.SubscriberCount,
                    Change = analytics.Change.GetNullable(channel.Name)?.actual,
                    LifetimeMAG = analytics.LifetimeMonthlyAverageGrowth.GetNullable(channel.Name),
                    RecentMAG = analytics.RecentMonthlyAverageGrowth.GetNullable(channel.Name),
                    SixMonthPrediction = analytics.Prediction.GetNullable(channel.Name),
                }) ;
            }

            var report = new Dictionary<string, Dictionary<string, ChannelWithAnalytics[]>>();
            foreach (var group in groupStructure)
            {
                var subGroups = new Dictionary<string, ChannelWithAnalytics[]>();
                foreach (var subGroup in group.Value)
                {
                    subGroups.Add(subGroup, channelsWithAnalytics.Where(channel => channel.SubGroup == subGroup).ToArray());
                }

                report.Add(group.Key, subGroups);
            }

            var json = JsonConvert.SerializeObject(report, Formatting.Indented);

            Directory.CreateDirectory($"{baseDirectory}/Results/Analytics");
            Directory.SetCurrentDirectory($"{baseDirectory}/Results/Analytics");

            File.WriteAllText($"{DateTime.Now.ToShortDateString().Replace("/","").Replace("-", "")}.json", json);
        }

        private void PrepareFileStructure()
        {
            Directory.CreateDirectory("./Results/");

            foreach (var group in groupStructure)
            {
                Directory.CreateDirectory($"./Results/{group.Key}");
            }
        }
    }
}
