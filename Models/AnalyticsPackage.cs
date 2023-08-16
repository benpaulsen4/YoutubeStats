namespace YoutubeStats.Models
{
    public class AnalyticsPackage
    {
        public Dictionary<string, (int actual, double percentage)> Change { get; init; }
        public Dictionary<string, int> LifetimeMonthlyAverageGrowth { get; init; }
        public Dictionary<string, int> RecentMonthlyAverageGrowth { get; init; }
        public Dictionary<string, int> Prediction { get; init; }

        public List<Award> Awards { get; init; }

        public AnalyticsPackage()
        {
            Change = new();
            LifetimeMonthlyAverageGrowth = new();
            RecentMonthlyAverageGrowth = new();
            Prediction = new();
            Awards = new();
        }
    }
}
