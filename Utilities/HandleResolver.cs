using Newtonsoft.Json.Linq;
using YoutubeStats.Models;

namespace YoutubeStats.Utilities
{
    internal static class HandleResolver
    {
        private static string BaseUrl => "https://yt.lemnoslife.com/channels?handle=";

        private static readonly HttpClient HttpClient = new();

        public static async Task ResolveHandles(IEnumerable<ChannelSummary> channelsWithHandles)
        {
            try
            {
                foreach (var channel in channelsWithHandles)
                {
                    channel.Id = await GetIdFromHandle(channel.Id);
                }
            }
            catch (HandleResolutionException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new HandleResolutionException(ResolutionErrorType.Unknown);
            }
        }

        private static async Task<string> GetIdFromHandle(string handle)
        {
            var response = await HttpClient.GetAsync(BaseUrl + handle);
            
            if (response.IsSuccessStatusCode)
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());

                string id;

                try
                {
                    id = json["items"]![0]!["id"]!.ToString();
                }
                catch (NullReferenceException) 
                {
                    //Note: The external API exhibits interesting behavior. Rather than always return 404 if a channel
                    // with that handle can't be found, it will try to find a channel with a *close enough* handle
                    // and return that instead. So this should almost never be thrown.
                    throw new HandleResolutionException(ResolutionErrorType.ChannelNotFound);
                }

                return id;
            }

            throw new HandleResolutionException(ResolutionErrorType.ExternalServiceUnavailable);
        }
    }

    public class HandleResolutionException : Exception
    {
        public ResolutionErrorType ErrorType { get; set; }

        public HandleResolutionException(ResolutionErrorType errorType) : base($"Channel handle resolution failure: {errorType}")
        {
            ErrorType = errorType;
        }
    }

    public enum ResolutionErrorType
    {
        ExternalServiceUnavailable,
        ChannelNotFound,
        Unknown
    }
}

// Example of response from third-party handle resolution API:
//{
//    "kind": "youtube#channelListResponse",
//    "etag": "NotImplemented",
//    "items": [
//        {
//            "kind": "youtube#channel",
//            "etag": "NotImplemented",
//            "id": "UC07-dOwgza1IguKA86jqxNA"
//        }
//    ]
//}

