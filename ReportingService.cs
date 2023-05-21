namespace YoutubeStats
{
    public class ReportingService
    {   
        private readonly ChannelSummary[] data;
        private readonly Dictionary<string, string[]> groupStructure;

        public ReportingService(ChannelSummary[] data, Dictionary<string, string[]> groupStructure)
        {
            this.data = data;
            this.groupStructure = groupStructure;
        }

        public void GenerateConsoleReport(bool showChange = false, Dictionary<string, (int, double)>? changes = null)
        {
            Console.WriteLine();
            Console.WriteLine($"Youtube Stats Report, {DateTime.Now}");
            Console.WriteLine();

            //foreach (var group in data)
            //{
            //    Console.WriteLine(group.Key);
            //    Console.WriteLine("==============================");
            //    foreach (var generation in group.Value)
            //    {
            //        var average = generation.Value.Where(channel => channel.IgnoreInAverage != true).Select(channel => channel.SubscriberCount).Average();
            //        Console.WriteLine("    " + $"{generation.Key} (Avg. {average:n0})");
            //        Console.WriteLine("    --------------------");

            //        var sortedChannels = generation.Value.ToList();
            //        sortedChannels.Sort();
            //        sortedChannels.Reverse();

            //        foreach (var channel in sortedChannels)
            //        {
            //            if (channel.SubscriberCount != null)
            //            {
            //                if (showChange)
            //                {
            //                    if (changes?.TryGetValue(channel.Name, out var change) == true)
            //                    {
            //                        Console.WriteLine("        " + $"{channel.Name} -> {channel.SubscriberCount:n0} : {change.Item1:n0} {(change.Item1 > 0 ? "increase": "decrease")} ({change.Item2:n3}%)");
            //                    }
            //                    else
            //                    {
            //                        Console.WriteLine("        " + $"{channel.Name} -> {channel.SubscriberCount:n0} : change unknown)");
            //                    }

            //                }
            //                else
            //                {
            //                    Console.WriteLine("        " + $"{channel.Name} -> {channel.SubscriberCount:n0}");
            //                }
            //            }
            //            else
            //            {
            //                Console.WriteLine("        " + $"{channel.Name} -> API could not find channel with this ID!");
            //            }
            //        }

            //        Console.WriteLine();
            //    }
            //}
        }

        public string GenerateCsvReport(bool skipConsole = false)
        {
            Console.WriteLine("Saving CSV...");

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

        public void GenerateAnalyticsReport()
        {
            var baseDirectory = GenerateCsvReport(true);
            Dictionary<string, (int, double)> changes = new();

            Console.WriteLine("Processing analytics...");

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
                                    changes.Add(channel.Key, (exactChange, percentChange));
                                    break;
                                }
                            }
                        }
                    }

                    try
                    {
                        ChartingService.GenerateSubGroupGraph(previousResults, subGroup);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error generating {subGroup} sub-group graph: {e.Message}");
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
                            subGroupAverages.Add(row.Date, new Dictionary<string, int>() { { subGroup, (int)row.Values.Where(channel => !averageIgnored.Contains(channel.Key)).Select(channel => channel.Value).Average() } }); ;
                        }

                    }
                }

                try
                {
                    ChartingService.GenerateGroupGraph(subGroupAverages, group.Key);
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

            foreach (var group in groupStructure)
            {
                Directory.CreateDirectory($"./Results/{group.Key}");
            }
        }
    }
}
