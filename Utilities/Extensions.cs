// Ignore Spelling: Nullable

using System.Text;
using YoutubeStats.Models;

namespace YoutubeStats.Utilities
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

        public static (int first, int second, int third) GetAwardCount(this IEnumerable<Award> awards, string name)
        {
            var first = awards.Where(award => award.Recipients.FirstOrDefault()?.Name == name).Count();
            var second = awards.Where(award => award.Recipients.ElementAtOrDefault(1)?.Name == name).Count();
            var third = awards.Where(award => award.Recipients.ElementAtOrDefault(2)?.Name == name).Count();

            return (first, second, third);
        }

        public static string GetSpacedEnum(this Enum enumerator)
        {
            var text = enumerator.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
