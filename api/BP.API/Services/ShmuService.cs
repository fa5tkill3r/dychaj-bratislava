using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models.Shmu;
using Microsoft.EntityFrameworkCore;

namespace BP.API.Services;

public class ShmuService
{
    private readonly Context _context;
    private readonly ILogger _logger;
    private readonly GoogleService _googleService;

    public ShmuService(Context context, ILogger logger, GoogleService googleService)
    {
        _context = context;
        _logger = logger;
        _googleService = googleService;
    }

    public async Task GetData()
    {
        var modules = await _context.Module
            .Where(m => m.Source == Source.Shmu)
            .Include(m => m.Sensors)
            .ToListAsync();

        foreach (var module in modules)
        {
            var shmuUriBuilder = new UriBuilder($"https://www.shmu.sk/api/v1/airquality/getdata?station=99133&history=1&station_meta=1");
            var station = await Requests.Get<List<ShmuResponse>>("https://www.shmu.sk/api/v1/airquality/getdata?&");
    
        }
    }

    public async Task<Module> AddModule(Station station)
    {
        var shmuResponses = await Requests.Get<List<ShmuResponse>>($"https://www.shmu.sk/api/v1/airquality/getdata?station={station.station_id}&history=1&station_meta=1");
        var shmuResponse = shmuResponses?.FirstOrDefault();
        if (shmuResponse == null)
        {
            _logger.LogError("ShmuService: Failed to get data");
            throw new Exception("Failed to get data");
        }
        
        Location? location = null;
        if (shmuResponse.station.gps_lat == null || shmuResponse.station.gps_lon == null)
        {
            _logger.LogInformation("ShmuService: Failed to location for station {StationId}", station.station_id);
        }
        else
        {
            var loc = await _googleService.GetLocation((double) shmuResponse.station.gps_lat, (double) shmuResponse.station.gps_lon);
            location = new Location()
            {
                Address = loc.Address,
                Name = loc.StreetName,
                Latitude = loc.Latitude,
                Longitude = loc.Longitude,
            };
        }
        

        var module = new Module()
        {
            Name = station.station_name,
            UniqueId = station.station_id.ToString(),
            Location = location,
            Source = Source.Shmu,
        };
        await _context.Module.AddAsync(module);  
        
        foreach (var stationPollutant in shmuResponse.station.pollutants)
        {
            var sensor = new Sensor()
            {
                UniqueId = stationPollutant.pollutant_id,
                Description = stationPollutant.pollutant_desc,
                Module = module,
            };
            await _context.Sensor.AddAsync(sensor);
        }
        
        await _context.SaveChangesAsync();
        
        return module;
    }

    public async Task<List<Station>> GetStations()
    {
        var response = await Requests.Get<List<ShmuResponse>>("https://www.shmu.sk/api/v1/airquality/getdata?&history=0&station_meta=1");
        if (response == null || !response.Any())
        {
            _logger.LogError("ShmuService: Failed to get data");
            throw new Exception("Failed to get data");
        }
        
        return response.Select(r => r.station).ToList();
    }
}