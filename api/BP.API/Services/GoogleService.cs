using BP.API.Utility;
using BP.Data.Models.GeoCode;
using Location = BP.Data.Models.Google.Location;

namespace BP.API.Services;

public class GoogleService
{
    private readonly ConfigurationRoot _configuration;
    private readonly ILogger<GoogleService> _logger;

    public GoogleService(ConfigurationRoot configuration, ILogger<GoogleService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Location?> GetLocation(double latitude, double longitude)
    {
        var apiKey = _configuration["APIKeys:Google"];
        
        var locationResponse = await Requests.Get<GeocodeResponse>($"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={apiKey}");
        if (locationResponse == null || locationResponse.results.Count == 0)
        {
            _logger.LogWarning("SensorCommunityService: Failed to get location for lat {Latitude} and long {Longitude}", latitude, longitude);
        }
        else
        {
            return new Location()
            {
                Latitude = latitude,
                Longitude = longitude,
                Address = locationResponse.results[0].formatted_address,
                StreetName = locationResponse.results[0].address_components.FirstOrDefault(a => a.types.Contains("route"))?.long_name ?? string.Empty
            };
        }
        
        return null;
    }
}