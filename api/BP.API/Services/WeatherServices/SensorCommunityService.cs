﻿using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models;
using BP.Data.Models.SensorCommunity;
using Microsoft.EntityFrameworkCore;
using Location = BP.Data.DbModels.Location;
using Sensor = BP.Data.DbModels.Sensor;

namespace BP.API.Services.WeatherServices;

public class SensorCommunityService : IWeatherService
{
    private readonly BpContext _bpContext;
    private readonly IConfiguration _configuration;
    private readonly GoogleService _googleService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorCommunityService> _logger;

    public SensorCommunityService(BpContext bpContext, ILogger<SensorCommunityService> logger,
        IConfiguration configuration, GoogleService googleService, IServiceScopeFactory scopeFactory)
    {
        _bpContext = bpContext;
        _logger = logger;
        _configuration = configuration;
        _googleService = googleService;
        _scopeFactory = scopeFactory;
    }

    public async Task GetData()
    {
        var modules = await _bpContext.Module
            .Where(m => m.Source == Source.SensorCommunity)
            .Include(m => m.Sensors)
            .ToListAsync();
        var sensors = modules.SelectMany(m => m.Sensors).ToList();
        var uniqueIds = sensors.Select(s => s.UniqueId).Distinct().ToList();

        var fetchData = new Func<string, Task>(async uniqueId =>
        {
            var scope = _scopeFactory.CreateScope();
            var bpContext = scope.ServiceProvider.GetRequiredService<BpContext>();
            var uniqueSensors = sensors.Where(s => s.UniqueId == uniqueId).ToList();

            _logger.LogInformation("SensorCommunityService: Getting data for sensor {SensorUniqueId}", uniqueId);

            var response =
                await Requests.Get<List<SensorCommunity>>(
                    $"https://data.sensor.community/airrohr/v1/sensor/{uniqueId}/");
            if (response == null)
            {
                _logger.LogError("SensorCommunityService: Failed to get data for sensor {SensorUniqueId}",
                    uniqueId);
                return;
            }

            foreach (var sensorCommunity in response)
            foreach (var dataValue in sensorCommunity.sensordatavalues)
            {
                var sensor =
                    uniqueSensors.FirstOrDefault(s => s.Type == Helpers.GetTypeFromString(dataValue.value_type));
                if (sensor == null)
                {
                    _logger.LogWarning(
                        "SensorCommunityService: Sensor {SensorUniqueId} has no sensor of type {ValueType}", uniqueId,
                        dataValue.value_type);
                    continue;
                }

                var isReadingInDb = await bpContext.Reading.AnyAsync(r =>
                    r.SensorId == sensor.Id && r.DateTime == sensorCommunity.timestamp);
                if (isReadingInDb)
                    continue;

                var reading = new Reading
                {
                    SensorId = sensor.Id,
                    DateTime = sensorCommunity.timestamp,
                    Value = dataValue.value
                };
                await bpContext.Reading.AddAsync(reading);
                await bpContext.SaveChangesAsync();
            }
        });

        var tasks = uniqueIds.Select(uniqueId => fetchData(uniqueId)).ToList();
        await Task.WhenAll(tasks);
    }

    public async Task AddSensor(Module module, string sensorId)
    {
        if (_bpContext.Sensor.Any(s => s.UniqueId == sensorId))
        {
            _logger.LogError("SensorCommunityService: Sensor {SensorUniqueId} already exists", sensorId);
            throw new Exception("Sensor already exists");
        }

        module.Source = Source.SensorCommunity;

        var response =
            await Requests.Get<List<SensorCommunity>>(
                $"https://data.sensor.community/airrohr/v1/sensor/{sensorId}/");
        if (response == null || response.Count == 0)
        {
            _logger.LogError("SensorCommunityService: Sensor {SensorUniqueId} not found", sensorId);
            throw new Exception("Sensor not found");
        }

        var sensorCommunity = response[0];

        foreach (var dataValue in sensorCommunity.sensordatavalues)
        {
            var sensor = new Sensor
            {
                Module = module,
                UniqueId = sensorId,
                Type = Helpers.GetTypeFromString(dataValue.value_type),
                Name = sensorCommunity.sensor.sensor_type.name
            };

            if (module.Location == null)
            {
                module.Location = new Location
                {
                    Latitude = sensorCommunity.location.latitude,
                    Longitude = sensorCommunity.location.longitude
                };

                var apiKey = _configuration["APIKeys:Google"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning(
                        "SensorCommunityService: Failed to get location for sensor {SensorUniqueId} - No API key",
                        sensorId);
                }
                else
                {
                    var loc = await _googleService.GetLocation(module.Location.Latitude, module.Location.Longitude);
                    if (loc == null)
                    {
                        _logger.LogWarning("SensorCommunityService: Failed to get location for sensor {SensorUniqueId}",
                            sensorId);
                    }
                    else
                    {
                        module.Location.Address = loc.Address;
                        module.Location.Name = loc.StreetName;
                        _logger.LogInformation(
                            "SensorCommunityService: Found location for sensor {SensorUniqueId} - {Address}", sensorId,
                            module.Location.Address);
                    }
                }
            }

            await _bpContext.Sensor.AddAsync(sensor);
        }


        await _bpContext.SaveChangesAsync();
    }

    public async Task<List<GetSensorsDto>> GetSensors()
    {
        var sensors =
            await Requests.Get<List<SensorCommunity>>("https://maps.sensor.community/data/v2/data.24h.json");

        if (sensors == null)
        {
            _logger.LogError("SensorCommunityService: Failed to get sensors");
            throw new Exception("Failed to get sensors");
        }
        
        return sensors.Select(sensor => new GetSensorsDto() {Name = sensor.id.ToString(), UniqueId = sensor.id.ToString(), Type = Helpers.GetTypeFromString(sensor.sensordatavalues[0].value_type),}).ToList();
    }
}