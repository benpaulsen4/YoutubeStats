namespace YoutubeStats
{
    internal static class ChartingService
    {
        public static void GenerateSubGroupGraph(List<CsvRow> subGroupCsv, string subGroupName)
        {
            List<double> x = new();
            Dictionary<string, List<double>> y = new();

            foreach (var row in subGroupCsv)
            {
                x.Add(row.Date.ToOADate());

                foreach (var channel in row.Values)
                {
                    if (y.TryGetValue(channel.Key, out var doubles))
                    {
                        doubles.Add(channel.Value);
                    }
                    else
                    {
                        y.Add(channel.Key, new List<double>() { channel.Value });
                    }
                }
            }

            var plt = new ScottPlot.Plot(1500, 900);
            var xArray = x.ToArray();

            foreach (var line in y)
            {
                plt.AddScatter(xArray, line.Value.ToArray(), label: line.Key, markerSize: 7, lineWidth: 2);
            }

            plt.XAxis.DateTimeFormat(true);
            plt.XAxis.LabelStyle(fontSize: 24);
            plt.YAxis.Label("Subscribers");
            plt.YAxis.LabelStyle(fontSize: 24);

            plt.XAxis.TickLabelStyle(fontSize: 18);
            plt.YAxis.TickLabelStyle(fontSize: 18);
            plt.XAxis.MajorGrid(lineWidth: 2);
            plt.YAxis.MajorGrid(lineWidth: 2);

            var legend = plt.Legend();
            legend.FontSize = 20;
            legend.Orientation = ScottPlot.Orientation.Horizontal;

            plt.SaveFig($"{subGroupName}.png");
        }

        public static void GenerateGroupGraph(Dictionary<DateTime, Dictionary<string, int>> subGroupAveragesUnsorted, string groupName)
        {
            var subGroupAverages = subGroupAveragesUnsorted.OrderBy(item => item.Key);
            double[] x = subGroupAverages.Select(item => item.Key.ToOADate()).ToArray();
            Dictionary<string, List<double>> y = new();
            Dictionary<string, double> offsets = new();

            foreach (var record in subGroupAverages)
            {
                foreach (var subGroupAverage in record.Value)
                {
                    var offsetExists = offsets.TryGetValue(subGroupAverage.Key, out var currentOffset);
                    if (!offsetExists)
                    {
                        offsets.Add(subGroupAverage.Key, record.Key.ToOADate());
                    }
                    else if (currentOffset > record.Key.ToOADate())
                    {
                        offsets[subGroupAverage.Key] = record.Key.ToOADate();
                    }

                    if (y.TryGetValue(subGroupAverage.Key, out var doubles))
                    {
                        doubles.Add(subGroupAverage.Value);
                    }
                    else
                    {
                        y.Add(subGroupAverage.Key, new List<double>() { subGroupAverage.Value });
                    }
                }
            }

            //pad nulls to beginning
            foreach (var offset in offsets)
            {
                var index = Array.IndexOf(x, offset.Value);
                if (index > 0)
                {
                    List<double> padded = new();
                    for (int i = 0; i < index; i++)
                    {
                        padded.Add(double.NaN);
                    }
                    padded.AddRange(y[offset.Key]);
                    y[offset.Key] = padded;
                }
            }

            var plt = new ScottPlot.Plot(1500, 900);
            var xArray = x.ToArray();

            foreach (var line in y)
            {
                var scatter = plt.AddScatter(xArray, line.Value.ToArray(), label: line.Key, markerSize: 7, lineWidth: 2);
                scatter.OnNaN = ScottPlot.Plottable.ScatterPlot.NanBehavior.Gap;
            }

            plt.XAxis.DateTimeFormat(true);
            plt.XAxis.LabelStyle(fontSize: 24);
            plt.YAxis.Label("Average Subscribers");
            plt.YAxis.LabelStyle(fontSize: 24);

            plt.XAxis.TickLabelStyle(fontSize: 18);
            plt.YAxis.TickLabelStyle(fontSize: 18);
            plt.XAxis.MajorGrid(lineWidth: 2);
            plt.YAxis.MajorGrid(lineWidth: 2);

            var legend = plt.Legend();
            legend.FontSize = 20;
            legend.Orientation = ScottPlot.Orientation.Horizontal;

            plt.SaveFig($"{groupName}.png");
        }
    }
}
