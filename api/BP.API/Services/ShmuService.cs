using System.Web;
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
            var shmuUriBuilder = new UriBuilder($"https://www.shmu.sk/api/v1/airquality/");
            
            var query = HttpUtility.ParseQueryString(shmuUriBuilder.Query);
            query["station"] = module.UniqueId;
            query["history"] = "1";
            
            var station = await Requests.Get<List<ShmuResponse>>(shmuUriBuilder.ToString());
            
            if (station == null)
            {
                _logger.LogError("ShmuService: Failed to get data for station {StationId}", module.UniqueId);
                continue;
            }
            
            foreach (var shmuResponse in station)
            {
                foreach (var pollutant in shmuResponse.station.pollutants)
                {
                    var sensor = _context.Sensor.FirstOrDefault(s => s.UniqueId == pollutant.pollutant_id);
                    if (sensor == null)
                    {
                        _logger.LogInformation("ShmuService: Sensor {SensorId} not found", pollutant.pollutant_id);
                        continue;
                    }
                    
                    var pollutantData = shmuResponse.data.FirstOrDefault(d => d.pollutant_id == pollutant.pollutant_id);
                    if (pollutantData == null)
                    {
                        _logger.LogWarning("ShmuService: Pollutant {PollutantId} not found", pollutant.pollutant_id);
                        continue;
                    }
                    
                    var isReadingInDb = await _context.Reading.AnyAsync(r =>
                        r.SensorId == sensor.Id && r.DateTime == DateTimeOffset.FromUnixTimeSeconds(pollutantData.dt));
                    
                    if (isReadingInDb)
                        continue;

                    var reading = new Reading()
                    {
                        Sensor = sensor,
                        DateTime = DateTimeOffset.FromUnixTimeSeconds(pollutantData.dt).DateTime,
                        Value = pollutantData.value
                    };
                    
                    await _context.Reading.AddAsync(reading);
                    await _context.SaveChangesAsync();
                }
            }
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
            
            if (loc == null)
            {
                _logger.LogInformation("ShmuService: Failed to get location for station {StationId}", station.station_id);
            }
            else
            {
                location = new Location()
                {
                    Address = loc.Address,
                    Name = loc.StreetName,
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude,
                };
            }
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