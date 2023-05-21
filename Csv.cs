namespace YoutubeStats
{
    internal class Csv
    {
        public static void Export(string subGroupName, ChannelSummary[] subGroup)
        {
            var writeHeader = !File.Exists($"{subGroupName}.csv");

            using (var writer = File.AppendText($"{subGroupName}.csv"))
            {
                if (writeHeader)
                {
                    string header = "date,";
                    for (int i = 0; i < subGroup.Length; i++)
                    {
                        header += subGroup[i].Name + (i == subGroup.Length - 1 ? "" : ",");
                    }

                    writer.WriteLine(header);
                }

                string line = DateTime.Now.ToShortDateString() + ",";

                for (int i = 0; i < subGroup.Length; i++)
                {
                    line += subGroup[i].SubscriberCount + (i == subGroup.Length - 1 ? "" : ",");
                }

                writer.WriteLine(line);
            }
        }

        public static List<CsvRow> Read(string subGroupName)
        {
            List<CsvRow> records = new();
            using (var reader = File.OpenText($"{subGroupName}.csv"))
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
