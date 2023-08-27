namespace YoutubeStats.Models
{
    public class Award
    {
        public AwardType Name { get; init; }
        public AwardUnit Unit { get; init; }
        public List<Recipient> Recipients { get; init; } = null!;
    }

    public record Recipient
    {
        public string Name { get; init; } = null!;
        public int? IntegerValue { get; init; }
        public double? DoubleValue { get; init; }
    }

    public enum AwardType
    {
        HighestRealGrowth,
        HighestRelativeGrowth,
        BestRecentPerformance,
        BreakoutStars,
        MostSubscribers,
        MostImproved
    }

    public enum AwardUnit
    {
        Subscribers,
        Percentage,
        Times
    }
}
