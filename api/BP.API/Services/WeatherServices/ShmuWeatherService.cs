using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models;
using BP.Data.Models.Shmu;
using Microsoft.EntityFrameworkCore;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Services.WeatherServices;

public class ShmuWeatherService : IWeatherService
{
    private readonly BpContext _bpContext;
    private readonly GoogleService _googleService;
    private readonly ILogger<ShmuWeatherService> _logger;

    public ShmuWeatherService(ILogger<ShmuWeatherService> logger, BpContext bpContext, GoogleService googleService)
    {
        _logger = logger;
        _bpContext = bpContext;
        _googleService = googleService;
    }

    public async Task GetData()
    {
        var shmuResponses =
            await Requests.Get<ShmuWeatherResponse>("https://www.shmu.sk/api/v1/meteo/getweathergeojson");
        if (shmuResponses == null)
        {
            _logger.LogError("ShmuWeatherService: Failed to get data");
            return;
        }

        var sensors = await _bpContext.Sensor
            .Where(s => s.Module.Source == Source.ShmuWeather)
            .ToListAsync();

        foreach (var sensor in sensors)
        {
            var feature = shmuResponses.features.FirstOrDefault(f => f.id == int.Parse(sensor.UniqueId));
            if (feature == null)
            {
                _logger.LogError("ShmuWeatherService: Sensor {SensorId} not found", sensor.UniqueId);
                continue;
            }

            if (sensor.Type == ValueType.Pressure)
            {
                if (decimal.TryParse(feature.properties.prop_weather.tlak, out var pressure))
                    await _bpContext.Reading.AddAsync(new Reading
                    {
                        Sensor = sensor,
                        Value = pressure,
                        DateTime = DateTimeOffset.FromUnixTimeSeconds(feature.properties.prop_weather.dt).UtcDateTime
                    });
                else
                    _logger.LogError("ShmuWeatherService: Sensor {SensorId} has invalid pressure data",
                        sensor.UniqueId);
            }
            else if (sensor.Type == ValueType.Temp)
            {
                if (feature.properties.prop_weather.ttt == null)
                {
                    _logger.LogError("ShmuWeatherService: Sensor {SensorId} has no temperature data", sensor.UniqueId);
                    continue;
                }

                await _bpContext.Reading.AddAsync(new Reading
                {
                    Sensor = sensor,
                    Value = (decimal) feature.properties.prop_weather.ttt,
                    DateTime = DateTimeOffset.FromUnixTimeSeconds(feature.properties.prop_weather.dt).UtcDateTime
                });
            }
            else
            {
                _logger.LogError("ShmuWeatherService: Sensor {SensorId} has unknown type", sensor.UniqueId);
            }
        }

        await _bpContext.SaveChangesAsync();
    }

    public async Task AddSensor(Module module, string uniqueId)
    {
        var shmuResponses =
            await Requests.Get<ShmuWeatherResponse>("https://www.shmu.sk/api/v1/meteo/getweathergeojson");
        if (shmuResponses == null)
        {
            _logger.LogError("ShmuWeatherService: Failed to get data for station {StationId}", module.UniqueId);
            return;
        }

        var feature = shmuResponses.features.FirstOrDefault(f => f.id == int.Parse(uniqueId));
        if (feature == null)
        {
            _logger.LogError("ShmuWeatherService: Sensor {SensorId} not found", uniqueId);
            return;
        }

        module.Source = Source.ShmuWeather;
        module.Name = feature.properties.prop_name;


        await _bpContext.Entry(module).Reference(m => m.Location).LoadAsync();

        if (module.Location == null)
        {
            var loc = await _googleService.GetLocation(feature.geometry.coordinates[1],
                feature.geometry.coordinates[0]);

            if (loc == null)
                _logger.LogInformation("ShmuService: Failed to get location for station {StationId}", uniqueId);
            else
                module.Location = new Location
                {
                    Address = loc.Address,
                    Name = loc.StreetName,
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude
                };
        }

        if (feature.properties.prop_weather.ttt != null)
        {
            var sensor = new Sensor
            {
                Module = module,
                UniqueId = feature.id.ToString(),
                Name = feature.properties.prop_name,
                Type = ValueType.Temp
            };
            await _bpContext.Sensor.AddAsync(sensor);
        }
        else
        {
            _logger.LogWarning("ShmuWeatherService: Sensor {SensorId} has no temperature", uniqueId);
        }

        if (feature.properties.prop_weather.tlak != null)
        {
            var sensor = new Sensor
            {
                Module = module,
                UniqueId = feature.id.ToString(),
                Name = feature.properties.prop_name,
                Type = ValueType.Pressure
            };
            await _bpContext.Sensor.AddAsync(sensor);
        }
        else
        {
            _logger.LogWarning("ShmuWeatherService: Sensor {SensorId} has no pressure", uniqueId);
        }

        await _bpContext.SaveChangesAsync();
    }

    public Task<List<GetSensorsDto>> GetSensors()
    {
        throw new NotImplementedException();
    }

    public Task FetchData(DateTime from, DateTime to, string? uniqueId)
    {
        throw new NotImplementedException();
    }
}