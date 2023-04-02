namespace YoutubeStats
{
    public class ChannelSummary : IComparable<ChannelSummary>
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int? SubscriberCount { get; set; }
        public bool? IgnoreInAverage { get; set; }

        public ChannelSummary(string id, string name, int? subscriberCount)
        {
            Id = id;
            Name = name;
            SubscriberCount = subscriberCount;
        }

        public int CompareTo(ChannelSummary? other)
        {
            if (other == null) return 1;
            if (SubscriberCount == null) return 1;
            if (other.SubscriberCount == null) return -1;

            return ((int)SubscriberCount).CompareTo((int)other.SubscriberCount);
        }
    }
}
