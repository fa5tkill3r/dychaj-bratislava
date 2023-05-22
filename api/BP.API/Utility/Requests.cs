using System.Net;
using Newtonsoft.Json;

namespace BP.API.Utility;

public static class Requests
{
    public static async Task<T?> Get<T>(string url)
    {
        var request = (HttpWebRequest) WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using var response = (HttpWebResponse) await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        using var reader = new StreamReader(stream);

        return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
    }
}