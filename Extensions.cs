// Ignore Spelling: Nullable

namespace YoutubeStats
{
    public static class Extensions
    {
        public static TValue? GetNullable<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey key) where TValue : struct
        {
            if (dictionary == null) return null;

            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            } 
            else
            {
                return null;
            }

        }
    }
}
