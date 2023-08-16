namespace YoutubeStats.Models
{
    public class Award
    {
        public AwardType Name { get; init; }
        public AwardUnit Unit { get; init; }
        public Recipient FirstPlace { get; init; } = null!;
        public Recipient? SecondPlace { get; init; }
        public Recipient? ThirdPlace { get; init; }
        public Recipient? FourthPlace { get; init; }
        public Recipient? FifthPlace { get; init; }
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
    }

    public enum AwardUnit
    {
        Subscribers,
        Percentage,
    }
}
