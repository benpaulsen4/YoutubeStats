namespace YoutubeStats
{
    internal class Csv
    {
        public static void Export(string genName, ChannelSummary[] generation)
        {
            var writeHeader = !File.Exists($"{genName}.csv");

            using (var writer = File.AppendText($"{genName}.csv"))
            {
                if (writeHeader)
                {
                    string header = "date,";
                    for (int i = 0; i < generation.Length; i++)
                    {
                        header += generation[i].Name + (i == generation.Length - 1 ? "" : ",");
                    }

                    writer.WriteLine(header);
                }

                string line = DateTime.Now.ToShortDateString() + ",";

                for (int i = 0; i < generation.Length; i++)
                {
                    line += generation[i].SubscriberCount + (i == generation.Length - 1 ? "" : ",");
                }

                writer.WriteLine(line);
            }
        }

        public static List<CsvRow> Read(string genName)
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

        public static void EraseLatestLine(string genName)
        {
            var lines = File.ReadAllLines($"{genName}.csv");
            File.WriteAllLines($"{genName}.csv", lines.Take(lines.Length - 1));
        }
    }

    internal class CsvRow
    {
        public DateTime Date { get; set; }
        public int Index { get; set; }
        public Dictionary<string, int> Values { get; set; } = new();
    }
}
