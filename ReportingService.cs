namespace YoutubeStats
{
    public class ReportingService
    {
        private readonly Dictionary<string, Dictionary<string, ChannelSummary[]>> data;

        public ReportingService(Dictionary<string, Dictionary<string, ChannelSummary[]>> data)
        {
            this.data = data;
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
                    var average = generation.Value.Select(channel => channel.SubscriberCount).Average();
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
                    ExportCsv(generation.Key, generation.Value);
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
            foreach (var group in data)
            {
                Directory.SetCurrentDirectory($"{baseDirectory}/Results/{group.Key}");

                var genAverages = new Dictionary<DateTime, Dictionary<string,int>>();

                foreach (var generation in group.Value)
                {
                    var previousResults = ReadCsv(generation.Key);

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
                            genAverages.Add(row.Date, new Dictionary<string, int>() { { generation.Key, (int)row.Values.Select(channel => channel.Value).Average() } });
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

        private void ExportCsv(string genName, ChannelSummary[] generation)
        {
            var writeHeader = !File.Exists($"{genName}.csv");

            using (var writer = File.AppendText($"{genName}.csv"))
            {
                if (writeHeader)
                {
                    string header = "date,";
                    for (int i=0; i< generation.Length; i++)
                    {
                        header += generation[i].Name + (i == generation.Length - 1 ? "" : ",");
                    }

                    writer.WriteLine(header);
                }

                string line = DateTime.Now.ToShortDateString() + ",";

                for (int i=0; i< generation.Length; i++)
                {
                    line += generation[i].SubscriberCount + (i == generation.Length - 1 ? "" : ",");
                }

                writer.WriteLine(line);
            }
        }

        private List<CsvRow> ReadCsv(string genName)
        {
            List<CsvRow> records = new();
            using (var reader = File.OpenText($"{genName}.csv"))
            {
                var headers = reader.ReadLine()!.Split(',');

                var index = 0;

                while (!reader.EndOfStream)
                {
                    var row = reader.ReadLine()!.Split(',');
                    CsvRow current = new()
                    {
                        Index = index++,
                        Date = DateTime.Parse(row[0])
                    };

                    for (int i = 1; i < row.Length; i++)
                    {
                        try
                        {
                            current.Values.Add(headers[i], int.Parse(row[i]));
                        }
                        catch (Exception)
                        {
                            //Skip row
                        }
                    }

                    records.Add(current);
                }
            }

            return records;
        }
    }

    public class CsvRow
    {
        public DateTime Date { get; set; }
        public int Index { get; set; }
        public Dictionary<string, int> Values { get; set; } = new();
    }
}
