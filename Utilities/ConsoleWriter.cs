using Spectre.Console;
using YoutubeStats.Models;

namespace YoutubeStats.Utilities
{
    public static class ConsoleWriter
    {
        public static void WriteReport(ChannelSummary[] data, Dictionary<string, string[]> groupStructure, AnalyticsPackage? package = null)
        {
            if (!AnsiConsole.Profile.Capabilities.Unicode)
            {
                AnsiConsole.MarkupLine("[yellow][b]Warning:[/] Your console does not support Unicode output, the report output may not look correct. Read more here: https://github.com/benpaulsen4/YoutubeStats/blob/master/readme.md#console-formatting [/]");
                AnsiConsole.WriteLine();
            }

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

            if (package != null)
            {
                AnsiConsole.Write(new Rule("Awards :trophy:") { Style = Style.Parse("yellow") });
                AnsiConsole.WriteLine();

                var grid = new Grid();
                grid.AddColumns(3);

                foreach (var chunk in package.Awards.Chunk(3))
                {
                    var awardRow = new List<Panel>();
                    foreach (var award in chunk)
                    {
                        awardRow.Add(GenerateAwardPanel(award));
                    }

                    grid.AddRow(awardRow.ToArray());
                }

                AnsiConsole.Write(grid);
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

                        table.AddRow(GetAwardSummaryString(package, member.Name), member.SubscriberCount!.Value.ToString("n0") ?? "", $"{(change.actual > 0 ? "[green]▲[/]" : change.actual == 0 ? "[blue]=[/]" : "[red]▼[/]")} {change.actual:n0}",
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

        private static Panel GenerateAwardPanel(Award award)
        {
            var content = $":1st_place_medal: [b]1 - {award.FirstPlace.Name}[/] ({GetAwardValueString(award.Unit, award.FirstPlace)})\n";
            if (award.SecondPlace != null) content += $":2nd_place_medal: [b]2 - {award.SecondPlace.Name}[/] ({GetAwardValueString(award.Unit, award.SecondPlace)})\n";
            if (award.ThirdPlace != null) content += $":3rd_place_medal: [b]3 - {award.ThirdPlace.Name}[/] ({GetAwardValueString(award.Unit, award.ThirdPlace)})\n";
            if (award.FourthPlace != null) content += $":glowing_star: [b]4 - {award.FourthPlace.Name}[/] ({GetAwardValueString(award.Unit, award.FourthPlace)})\n";
            if (award.FifthPlace != null) content += $":glowing_star: [b]5 - {award.FifthPlace.Name}[/] ({GetAwardValueString(award.Unit, award.FifthPlace)})";

            var panel = new Panel(content)
            {
                Header = new PanelHeader(award.Name.GetSpacedEnum()),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1)
            };

            return panel;
        }

        private static string GetAwardValueString(AwardUnit unit, Recipient recipient)
        {
            if (unit == AwardUnit.Percentage) return $"{recipient.DoubleValue:n2}%";

            return $"{recipient.IntegerValue:n0} subs";
        }

        private static string GetAwardSummaryString(AnalyticsPackage package, string name)
        {
            var (first, second, third) = package.Awards.GetAwardCount(name);
            if (first + second + third == 0) return name;

            return $"{name} {string.Concat(Enumerable.Repeat(":1st_place_medal:", first)) + string.Concat(Enumerable.Repeat(":2nd_place_medal:", second)) + string.Concat(Enumerable.Repeat(":3rd_place_medal:", third))}";
        }
    }
}
