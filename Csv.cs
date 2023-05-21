namespace YoutubeStats
{
    internal class Csv
    {
        public static void Export(string subGroupName, ChannelSummary[] subGroup)
        {
            var fileExists = File.Exists($"{subGroupName}.csv");
            var headers = new List<string>();

            if (fileExists)
            {
                using var reader = File.OpenText($"{subGroupName}.csv");
                headers = reader.ReadLine()!.Split(',').ToList();
                headers.Remove("date");
            }

            using var writer = File.AppendText($"{subGroupName}.csv");

            if (!fileExists)
            {
                string headerLine = "date,";
                for (int i = 0; i < subGroup.Length; i++)
                {
                    headerLine += subGroup[i].Name + (i == subGroup.Length - 1 ? "" : ",");
                    headers.Add(subGroup[i].Name);
                }

                writer.WriteLine(headerLine);
            }



            string line = DateTime.Now.ToShortDateString() + ",";

            // It is a known limitation here that this will not include data that does not already have a header in the file.
            // This is in-line with the limitation of the program in not being able to add new sub-group members on the fly.
            // The goal here is simply to make sure the order of channels is correct for each write.
            for (int i = 0; i < headers.Count; i++)
            {
                line += subGroup.First(channel => channel.Name == headers[i]).SubscriberCount + (i == subGroup.Length - 1 ? "" : ",");
            }

            writer.WriteLine(line);
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

        public static void EraseLatestLine(string subGroupName)
        {
            var lines = File.ReadAllLines($"{subGroupName}.csv");
            File.WriteAllLines($"{subGroupName}.csv", lines.Take(lines.Length - 1));
        }
    }

    internal class CsvRow
    {
        public DateTime Date { get; set; }
        public int Index { get; set; }
        public Dictionary<string, int> Values { get; set; } = new();
    }
}
