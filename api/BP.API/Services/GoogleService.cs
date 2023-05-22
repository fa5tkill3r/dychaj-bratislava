using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using BP.API.Utility;
using BP.Data.Models.Geocode;
using Location = BP.Data.Models.Google.Location;

namespace BP.API.Services;

public class GoogleService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleService> _logger;

    public GoogleService(IConfiguration configuration, ILogger<GoogleService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Location?> GetLocation(double latitude, double longitude)
    {
        var reqQuery = new NameValueCollection
        {
            {"lat", latitude.ToString(CultureInfo.InvariantCulture)},
            {"lng", longitude.ToString(CultureInfo.InvariantCulture)}
        };

        return await GetLocationFromResponse(reqQuery);
    }

    public async Task<Location?> GetLocation(string address)
    {
        var reqQuery = new NameValueCollection
        {
            {"address", address}
        };

        return await GetLocationFromResponse(reqQuery);
    }

    private async Task<Location?> GetLocationFromResponse(NameValueCollection reqQuery)
    {
        var apiKey = _configuration["APIKeys:Google"];

        var request = new UriBuilder("https://maps.googleapis.com/maps/api/geocode/json");

        var query = HttpUtility.ParseQueryString(request.Query);
        query["key"] = apiKey;
        query.Add(reqQuery);
        
        request.Query = query.ToString();

        var locationResponse = await Requests.Get<GeocodeResponse>(request.ToString());
        if (locationResponse == null || locationResponse.results.Count == 0)
            _logger.LogWarning("SensorCommunityService: Failed to get location");
        else
            return new Location
            {
                Latitude = locationResponse.results[0].geometry.location.lat,
                Longitude = locationResponse.results[0].geometry.location.lng,
                Address = locationResponse.results[0].formatted_address,
                StreetName =
                    locationResponse.results[0].address_components.FirstOrDefault(a => a.types.Contains("route"))
                        ?.long_name ?? string.Empty
            };

        return null;
    }
}