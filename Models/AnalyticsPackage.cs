namespace YoutubeStats.Models
{
    public class AnalyticsPackage
    {
        public Dictionary<string, (int actual, double percentage)> Change { get; set; }
        public Dictionary<string, int> LifetimeMonthlyAverageGrowth { get; set; }
        public Dictionary<string, int> RecentMonthlyAverageGrowth { get; set; }
        public Dictionary<string, int> Prediction { get; set; }

        public AnalyticsPackage()
        {
            Change = new();
            LifetimeMonthlyAverageGrowth = new();
            RecentMonthlyAverageGrowth = new();
            Prediction = new();
        }
    }
}
