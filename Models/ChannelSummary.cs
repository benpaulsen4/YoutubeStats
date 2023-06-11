using Newtonsoft.Json;

namespace YoutubeStats.Models
{
    public class ChannelSummary : IComparable<ChannelSummary>
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool? IgnoreInAverage { get; set; }
        [JsonIgnore] public int? SubscriberCount { get; set; }
        [JsonIgnore] public string? SubGroup { get; set; }

        public int CompareTo(ChannelSummary? other)
        {
            if (other == null) return 1;
            if (SubscriberCount == null) return 1;
            if (other.SubscriberCount == null) return -1;

            return ((int)SubscriberCount).CompareTo((int)other.SubscriberCount);
        }
    }
}
