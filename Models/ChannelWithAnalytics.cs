using Newtonsoft.Json;

namespace YoutubeStats.Models
{
    public record ChannelWithAnalytics
    {
        public string? Name { get; set; }
        public int? SubscriberCount { get; set; }
        [JsonIgnore] public string? SubGroup { get; set; }

        public int? Change { get; set; }
        public int? LifetimeMAG { get; set; }
        public int? RecentMAG { get; set; }
        public int? SixMonthPrediction { get; set; }
    }
}
