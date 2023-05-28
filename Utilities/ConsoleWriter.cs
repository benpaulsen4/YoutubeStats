using Spectre.Console;
using YoutubeStats.Models;

namespace YoutubeStats.Utilities
{
    public static class ConsoleWriter
    {
        public static void WriteReport(ChannelSummary[] data, Dictionary<string, string[]> groupStructure, AnalyticsPackage? package = null)
        {
            foreach (var group in groupStructure)
            {
                var table = new Table
                {
                    Title = new TableTitle(group.Key, new Style(foreground: Color.SeaGreen1)),
                    Border = TableBorder.DoubleEdge
                };

                table.AddColumn(new TableColumn("Subgroups").Centered());
                table.HideHeaders();

                foreach (var subGroup in group.Value)
                {
                    table.AddRow(GenerateSubGroupTable(subGroup, data.Where(channel => channel.SubGroup == subGroup).ToArray(), package));
                    table.AddEmptyRow();
                }

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }
        }

        private static Table GenerateSubGroupTable(string name, ChannelSummary[] members, AnalyticsPackage? package)
        {
            var subGroupAverage = members.Where(channel => channel.IgnoreInAverage != true).Select(channel => channel.SubscriberCount).Average();

            var table = new Table
            {
                Title = new TableTitle($"[bold]{name}[/] (Avg. {subGroupAverage:n0})", new Style(foreground: Color.LightSkyBlue1)),
                Border = TableBorder.Rounded
            };

            table.AddColumn("Channel");
            table.AddColumn("Subscribers");

            var sortedMembers = members.ToList();
            sortedMembers.Sort();
            sortedMembers.Reverse();

            if (package != null)
            {
                table.AddColumns("Change", "% Change", "Lifetime MAG", "Recent MAG", "Prediction");

                foreach (var member in sortedMembers)
                {
                    if (package.Change.TryGetValue(member.Name, out var change))
                    {
                        var lifetimeMAG = package.LifetimeMonthlyAverageGrowth.GetNullable(member.Name);
                        var recentMAG = package.RecentMonthlyAverageGrowth.GetNullable(member.Name);
                        var prediction = package.Prediction.GetNullable(member.Name);

                        table.AddRow(member.Name, member.SubscriberCount!.Value.ToString("n0") ?? "", $"{(change.actual > 0 ? "[green]▲[/]" : change.actual == 0 ? "[blue]=[/]" : "[red]▼[/]")} {change.actual:n0}", 
                            $"{change.percentage:n3}%", $"{lifetimeMAG:n0}", $"{recentMAG:n0}", $"{prediction:n0}");
                    }
                    else
                    {
                        table.AddRow(member.Name, member.SubscriberCount!.Value.ToString("n0") ?? "", "Unknown", "", "", "", "");
                    }
                }
            }
            else
            {
                foreach (var member in sortedMembers)
                {
                    table.AddRow(member.Name, member.SubscriberCount!.Value.ToString("n0") ?? "");
                }
            }

            table.Centered();
            return table;
        }
    }
}
