using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models.GeoCode;
using BP.Data.Models.SensorCommunity;
using Microsoft.EntityFrameworkCore;
using Location = BP.Data.DbModels.Location;
using Sensor = BP.Data.DbModels.Sensor;

namespace BP.API.Services;

public class SensorCommunityService
{
    private readonly Context _context;
    private readonly ILogger<SensorCommunityService> _logger;
    private readonly IConfigurationRoot _configuration;
    private readonly GoogleService _googleService;

    public SensorCommunityService(Context context, ILogger<SensorCommunityService> logger, IConfigurationRoot configuration, GoogleService googleService)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _googleService = googleService;
    }

    public async Task GetData()
    {
        var modules = await _context.Module
            .Where(m => m.Source == Source.SensorCommunity)
            .Include(m => m.Sensors)
            .ToListAsync();

        foreach (var sensor in modules.SelectMany(module => module.Sensors))
        {
            _logger.LogInformation("SensorCommunityService: Getting data for sensor {SensorUniqueId}", sensor.UniqueId);

            var response =
                await Requests.Get<List<SensorCommunity>>(
                    $"https://data.sensor.community/airrohr/v1/sensor/{sensor.UniqueId}/");
            if (response == null)
            {
                _logger.LogError("SensorCommunityService: Failed to get data for sensor {SensorUniqueId}",
                    sensor.UniqueId);
                continue;
            }

            foreach (var sensorCommunity in response)
            {
                var isReadingInDb = await _context.Reading.AnyAsync(r =>
                    r.SensorId == sensor.Id && r.DateTime == sensorCommunity.timestamp);
                if (isReadingInDb)
                    continue;

                var reading = new Reading()
                {
                    SensorId = sensor.Id,
                    DateTime = sensorCommunity.timestamp,
                    Value = sensorCommunity.sensordatavalues[0].value
                };
                await _context.Reading.AddAsync(reading);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<Sensor> AddSensor(Module module, string sensorId)
    {
        if (_context.Sensor.Any(s => s.UniqueId == sensorId))
        {
            _logger.LogError("SensorCommunityService: Sensor {SensorUniqueId} already exists", sensorId);
            throw new Exception("Sensor already exists");
        }
        
        var response =
            await Requests.Get<List<SensorCommunity>>(
                $"https://data.sensor.community/airrohr/v1/sensor/{sensorId}/");
        if (response == null || response.Count == 0)
        {
            _logger.LogError("SensorCommunityService: Sensor {SensorUniqueId} not found", sensorId);
            throw new Exception("Sensor not found");
        }

        var sensorCommunity = response[0];
        var sensor = new Sensor()
        {
            Module = module,
            UniqueId = sensorId,
            Unit = Helpers.GetUnitFromType(sensorCommunity.sensordatavalues[0].value_type),
            Name = sensorCommunity.sensor.sensor_type.name,
        };

        if (module.Location == null)
        {
            module.Location = new Location()
            {
                Latitude = sensorCommunity.location.latitude,
                Longitude = sensorCommunity.location.longitude,
            };
            
            var apiKey = _configuration["APIKeys:Google"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("SensorCommunityService: Failed to get location for sensor {SensorUniqueId} - No API key", sensorId);
            }
            else
            {
                var loc = await _googleService.GetLocation(module.Location.Latitude, module.Location.Longitude);
                if (loc == null)
                {
                    _logger.LogWarning("SensorCommunityService: Failed to get location for sensor {SensorUniqueId}", sensorId);
                }
                else
                {
                    module.Location.Address = loc.Address;
                    module.Location.Name = loc.StreetName;
                    _logger.LogInformation("SensorCommunityService: Found location for sensor {SensorUniqueId} - {Address}", sensorId, module.Location.Address);
                }
            }

        }
        
        await _context.Sensor.AddAsync(sensor);
        await _context.SaveChangesAsync();
        
        return sensor;
    }
}