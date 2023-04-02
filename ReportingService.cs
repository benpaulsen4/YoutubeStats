namespace YoutubeStats
{
    public class ReportingService
    {
        private readonly Dictionary<string, Dictionary<string, ChannelSummary[]>> data;
        private List<string> averageIgnored = new();

        public ReportingService(Dictionary<string, Dictionary<string, ChannelSummary[]>> data)
        {
            this.data = data;
        }

        private void CurateAverageIgnoredList()
        {
            averageIgnored = data.Select(data => data.Value).SelectMany(gens => gens.Values).SelectMany(channels => channels).Where(channel => channel.IgnoreInAverage == true).Select(channel => channel.Name).ToList();
        }

        public void GenerateConsoleReport(bool showChange = false, Dictionary<string, double>? changes = null)
        {
            Console.WriteLine();
            Console.WriteLine($"Youtube Stats Report, {DateTime.Now}");
            Console.WriteLine();

            foreach (var group in data)
            {
                Console.WriteLine(group.Key);
                Console.WriteLine("==============================");
                foreach (var generation in group.Value)
                {
                    var average = generation.Value.Where(channel => channel.IgnoreInAverage != true).Select(channel => channel.SubscriberCount).Average();
                    Console.WriteLine("    " + $"{generation.Key} (Avg. {average:n0})");
                    Console.WriteLine("    --------------------");

                    var sortedChannels = generation.Value.ToList();
                    sortedChannels.Sort();
                    sortedChannels.Reverse();

                    foreach (var channel in sortedChannels)
                    {
                        if (channel.SubscriberCount != null)
                        {
                            if (showChange)
                            {
                                if (changes?.TryGetValue(channel.Name, out var change) == true)
                                {
                                    Console.WriteLine("        " + $"{channel.Name} -> {channel.SubscriberCount:n0} ({change:n3}% change)");
                                }
                                else
                                {
                                    Console.WriteLine("        " + $"{channel.Name} -> {channel.SubscriberCount:n0} (Change unknown)");
                                }

                            }
                            else
                            {
                                Console.WriteLine("        " + $"{channel.Name} -> {channel.SubscriberCount:n0}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("        " + $"{channel.Name} -> API could not find channel with this ID!");
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        public string GenerateCsvReport(bool skipConsole = false)
        {
            Console.WriteLine("Generating report...");

            PrepareFileStructure();

            var baseDirectory = Directory.GetCurrentDirectory();

            foreach (var group in data)
            {
                Directory.SetCurrentDirectory($"{baseDirectory}/Results/{group.Key}");

                foreach (var generation in group.Value)
                {
                    Csv.Export(generation.Key, generation.Value);
                }
            }

            if (!skipConsole)
            {
                GenerateConsoleReport();
            }

            return baseDirectory;
        }

        public void GenerateAnalyticsReport()
        {
            var baseDirectory = GenerateCsvReport(true);
            Dictionary<string, double> changes = new();

            Console.WriteLine("Processing analytics...");

            CurateAverageIgnoredList();

            foreach (var group in data)
            {
                Directory.SetCurrentDirectory($"{baseDirectory}/Results/{group.Key}");

                var genAverages = new Dictionary<DateTime, Dictionary<string, int>>();

                foreach (var generation in group.Value)
                {
                    var previousResults = Csv.Read(generation.Key);

                    var maxIndex = previousResults.Max(row => row.Index);
                    var latestRow = previousResults.Where(row => row.Index == maxIndex).FirstOrDefault();
                    var previousRow = previousResults.Where(row => row.Index == maxIndex - 1).FirstOrDefault();

                    if (latestRow != null && previousRow != null)
                    {
                        foreach (var channel in latestRow.Values)
                        {
                            foreach (var prevChannel in previousRow.Values)
                            {
                                if (channel.Key == prevChannel.Key)
                                {
                                    double percentChange = ((channel.Value - prevChannel.Value) / (double)channel.Value) * 100;
                                    changes.Add(channel.Key, percentChange);
                                    break;
                                }
                            }
                        }
                    }

                    try
                    {
                        ChartingService.GenerateGenerationGraph(previousResults, generation.Key);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error generating {generation.Key} generation graph: {e.Message}");
                    }

                    foreach (var row in previousResults)
                    {
                        if (genAverages.TryGetValue(row.Date, out var averages))
                        {
                            averages.Add(generation.Key, (int)row.Values.Select(channel => channel.Value).Average());
                        }
                        else
                        {
                            genAverages.Add(row.Date, new Dictionary<string, int>() { { generation.Key, (int)row.Values.Where(channel => !averageIgnored.Contains(channel.Key)).Select(channel => channel.Value).Average() } }); ;
                        }

                    }
                }

                try
                {
                    ChartingService.GenerateGroupGraph(genAverages, group.Key);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error generating {group.Key} group graph: {e.Message}");
                }
            }

            GenerateConsoleReport(true, changes);
        }

        private void PrepareFileStructure()
        {
            Directory.CreateDirectory("./Results/");

            foreach (var group in data)
            {
                Directory.CreateDirectory($"./Results/{group.Key}");
            }
        }
    }
}
