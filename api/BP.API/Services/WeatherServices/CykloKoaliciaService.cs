﻿using BP.Data;
using BP.Data.CykloKoalicia;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using Microsoft.EntityFrameworkCore;
using Sensor = BP.Data.DbModels.Sensor;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Services.WeatherServices;

public class CykloKoaliciaService : IWeatherService
{
    private readonly BpContext _bpContext;
    private readonly CkVzduchContext _ckVzduchContext;
    private readonly GoogleService _googleService;
    private readonly ILogger<CykloKoaliciaService> _logger;

    public CykloKoaliciaService(BpContext bpContext,
        CkVzduchContext ckVzduchContext,
        GoogleService googleService,
        ILogger<CykloKoaliciaService> logger)
    {
        _bpContext = bpContext;
        _ckVzduchContext = ckVzduchContext;
        _googleService = googleService;
        _logger = logger;
    }

    public async Task GetData()
    {
        var moduleIds = await _bpContext.Module
            .Where(m => m.Source == Source.CykloKoalicia)
            .Select(m => m.Id)
            .ToListAsync();

        var sensors = await _bpContext.Sensor
            .Where(s => moduleIds.Contains(s.ModuleId))
            .ToListAsync();

        var sensorIds = sensors.Select(s => int.Parse(s.UniqueId)).ToList();
        var sensorValues = await _ckVzduchContext.SensorsValues
            .Where(value => sensorIds.Contains((int)value.SensorId))
            .GroupBy(value => value.SensorId)
            .ToDictionaryAsync(group => (int)group.Key, group => group.OrderByDescending(v => v.CreatedAt).FirstOrDefault());

        foreach (var sensor in sensors)
        {
            if (!sensorValues.TryGetValue(int.Parse(sensor.UniqueId), out var value) || value == null)
            {
                _logger.LogError("CykloKoaliciaService: Failed to get sensor value");
                continue;
            }

            var datetime = value.CreatedAt?.ToUniversalTime() ?? DateTime.UtcNow;

            switch (sensor.Type)
            {
                case ValueType.Humidity:
                    await CreateReading(sensor, value.Humidity, datetime);
                    break;
                case ValueType.Pressure:
                    await CreateReading(sensor, value.Pressure, datetime);
                    break;
                case ValueType.Temp:
                    await CreateReading(sensor, value.Temperature, datetime);
                    break;
                case ValueType.Pm10:
                    await CreateReading(sensor, value.Pm10, datetime);
                    break;
                case ValueType.Pm25:
                    await CreateReading(sensor, value.Pm25, datetime);
                    break;
            }
        }
    }



    public async Task AddSensor(Module module, string uniqueId)
    {
        module.Source = Source.CykloKoalicia;
        module.UniqueId = uniqueId;

        await _bpContext.Entry(module).Reference(m => m.Location).LoadAsync();

        var ckSensor = _ckVzduchContext.Sensors.FirstOrDefault(s => s.Number == uniqueId);

        if (ckSensor == null)
        {
            _logger.LogError("CykloKoaliciaService: Failed to get sensor");
            return;
        }

        if (module.Location == null)
        {
            var location = await _googleService.GetLocation(ckSensor.Location);
            if (location == null)
            {
                _logger.LogError("CykloKoaliciaService: Failed to get location");
                return;
            }

            module.Location = new Location
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Address = location.Address
            };
        }


        module.Name = ckSensor.Location;

        var sensorsValue = _ckVzduchContext.SensorsValues
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault(s => s.SensorId == ckSensor.Id);

        if (sensorsValue == null)
        {
            _logger.LogError("CykloKoaliciaService: Failed to get sensor value");
            return;
        }

        if (sensorsValue.Humidity != 0)
            await CreateSensor(module, sensorsValue, ValueType.Humidity, sensorsValue.Humidity);
        if (sensorsValue.Pm10 != 0)
            await CreateSensor(module, sensorsValue, ValueType.Pm10, sensorsValue.Pm10);
        if (sensorsValue.Pm25 != 0)
            await CreateSensor(module, sensorsValue, ValueType.Pm25, sensorsValue.Pm25);
        if (sensorsValue.Pressure != 0)
            await CreateSensor(module, sensorsValue, ValueType.Pressure, sensorsValue.Pressure);
        if (sensorsValue.Temperature != 0)
            await CreateSensor(module, sensorsValue, ValueType.Temp, sensorsValue.Temperature);

        await _bpContext.SaveChangesAsync();
    }

    private async Task CreateSensor(Module module, SensorsValue sensorsValue, ValueType valueType, decimal value)
    {
        var sensor = new Sensor()
        {
            ModuleId = module.Id,
            UniqueId = sensorsValue.SensorId.ToString(),
            Type = valueType,
            Name = $"{valueType} {sensorsValue.SensorId}"
        };
        await _bpContext.Sensor.AddAsync(sensor);
        await _bpContext.SaveChangesAsync();

        await _bpContext.Reading.AddAsync(new Reading()
        {
            Sensor = sensor,
            Value = value,
            DateTime = sensorsValue.CreatedAt?.ToUniversalTime() ?? DateTime.UtcNow,
        });
    }

    private async Task CreateReading(Sensor sensor, decimal value, DateTime dateTime)
    {
        await _bpContext.Reading.AddAsync(new Reading()
        {
            Sensor = sensor,
            Value = value,
            DateTime = dateTime,
        });
    }
}