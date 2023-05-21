using Spectre.Console;

namespace YoutubeStats
{
    public static class ConsoleWriter
    {
        public static void WriteReport(ChannelSummary[] data, Dictionary<string, string[]> groupStructure, Dictionary<string, (int, double)>? changes = null)
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
                    table.AddRow(GenerateSubGroupTable(subGroup, data.Where(channel => channel.SubGroup == subGroup).ToArray(), changes));
                    table.AddEmptyRow();
                }

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }
        }

        private static Table GenerateSubGroupTable(string name, ChannelSummary[] members, Dictionary<string, (int, double)>? changes)
        {
            var subGroupAverage = members.Where(channel => channel.IgnoreInAverage != true).Select(channel => channel.SubscriberCount).Average();

            var table = new Table
            {
                Title = new TableTitle($"[bold]{name}[/] (Avg. {subGroupAverage:n0})", new Style(foreground: Color.PaleVioletRed1)),
                Border = TableBorder.Rounded
            };

            table.AddColumn("Channel");
            table.AddColumn("Subscribers");

            var sortedMembers = members.ToList();
            sortedMembers.Sort();
            sortedMembers.Reverse();

            if (changes != null)
            {
                table.AddColumn("Change");
                table.AddColumn("% Change");

                foreach (var member in sortedMembers)
                {
                    if (changes?.TryGetValue(member.Name, out var change) == true)
                    {
                        table.AddRow(member.Name, member.SubscriberCount!.Value.ToString("n0") ?? "", $"{(change.Item1 > 0 ? "[green]▲[/]" : change.Item1 == 0 ? "[blue]=[/]" : "[red]▼[/]")} {change.Item1:n0}", $"{change.Item2:n3}%");
                    }
                    else
                    {
                        table.AddRow(member.Name, member.SubscriberCount!.Value.ToString("n0") ?? "", "Unknown", "");
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
