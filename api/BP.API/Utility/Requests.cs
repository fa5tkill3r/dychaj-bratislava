using System.Net;
using Newtonsoft.Json;

namespace BP.API.Utility;

public static class Requests
{
    public static async Task<T?> Get<T>(string url, int retries = 0)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }

            if (retries > 0)
            {
                return await Get<T>(url, retries - 1);
            }

            return default;
        }
        catch (Exception)
        {
            if (retries > 0)
            {
                return await Get<T>(url, retries - 1);
            }

            return default;
        }
    }

    public static async Task<Stream?> GetFileStream(string url)
    {
        try
        {
            using var client = new HttpClient();
            return await client.GetStreamAsync(url);
        }
        catch (Exception)
        {
            return null;
        }
    }
}