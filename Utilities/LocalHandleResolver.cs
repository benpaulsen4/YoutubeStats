using System.Net;
using System.Text.RegularExpressions;
using YoutubeStats.Models;

namespace YoutubeStats.Utilities;

public partial class LocalHandleResolver
{
    private static string YoutubeUrl => "https://youtube.com/";

    private readonly HttpClient httpClient = new();

    private readonly Regex channelIdRegex =
        MyRegex();

    public async Task ResolveHandles(IEnumerable<ChannelSummary> channelsWithHandles)
    {
        foreach (var channel in channelsWithHandles)
        {
            channel.Id = await GetIdFromHandle(channel.Id);
            //Try and avoid bot-like spamming
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }

    private async Task<string> GetIdFromHandle(string handle)
    {
        var response = await httpClient.GetAsync(YoutubeUrl + handle);

        if (response.StatusCode == HttpStatusCode.NotFound) throw new HandleResolutionException(handle);

        var responseHtml = await response.Content.ReadAsStringAsync();

        var linkMatch = channelIdRegex.Match(responseHtml);
        if (!linkMatch.Success) throw new Exception("Youtube HTML response could not be read");

        return linkMatch.Groups.Values.Last().Value;
    }

    public class HandleResolutionException : Exception
    {
        public string ChannelHandle { get; set; }

        public HandleResolutionException(string handle) : base($"Failed to find matching Youtube ID for '{handle}'")
        {
            ChannelHandle = handle;
        }
    }

    [GeneratedRegex("<link rel=\"canonical\" href=\"https://www\\.youtube\\.com/channel/(.{24})\">")]
    private static partial Regex MyRegex();
}