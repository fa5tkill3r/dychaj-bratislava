using BP.Data;
using BP.Data.CykloKoalicia;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models;
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
            .Include(s => s.Readings
                .OrderByDescending(r => r.DateTime)
                .Take(5))
            .ToListAsync();

        var sensorIds = sensors
            .Select(s => int.Parse(s.UniqueId))
            .Distinct()
            .ToList();
        var sensorValues = await _ckVzduchContext.SensorsValues
            .Where(value => sensorIds.Contains((int) value.SensorId))
            .Where(value => value.CreatedAt >= DateTime.UtcNow.AddDays(-1))
            .GroupBy(value => value.SensorId)
            .ToDictionaryAsync(group => (int) group.Key, group => group.MaxBy(v => v.CreatedAt));

        foreach (var sensor in sensors)
        {
            if (!sensorValues.TryGetValue(int.Parse(sensor.UniqueId), out var value) || value == null)
            {
                _logger.LogError(
                    "CykloKoaliciaService: Failed to get sensor value for {SensorUniqueId} with module {ModuleId}",
                    sensor.UniqueId, sensor.ModuleId);
                continue;
            }
            
            if (value.CreatedAt == null)
            {
                _logger.LogError(
                    "CykloKoaliciaService: Failed to get sensor datetime for {SensorUniqueId} with module {ModuleId}",
                    sensor.UniqueId, sensor.ModuleId);
                continue;
            }
            
            var ckCreatedAt = DateTime.SpecifyKind(value.CreatedAt.Value, DateTimeKind.Unspecified);
            var ckTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var datetime = TimeZoneInfo.ConvertTimeToUtc(ckCreatedAt, ckTimeZone);

            if (sensor.Readings.Any(r => r.DateTime == datetime))
            {
                _logger.LogInformation(
                    "CykloKoaliciaService: Sensor {SensorUniqueId} already has reading for {DateTime}", sensor.UniqueId,
                    datetime);
                continue;
            }

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

        await _bpContext.SaveChangesAsync();
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
                Name = location.StreetName,
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

    public async Task<List<GetSensorsDto>> GetSensors()
    {
        var distinctSensors = await _ckVzduchContext.SensorsValues
            .Where(s => s.CreatedAt > DateTime.UtcNow.AddDays(-1))
            .GroupBy(s => s.SensorId)
            .Select(s => s.OrderByDescending(sv => sv.CreatedAt).FirstOrDefault())
            .ToListAsync();

        var sensors = await _ckVzduchContext.Sensors
            .Where(s => distinctSensors.Select(ds => ds!.SensorId).Contains(s.Id))
            .ToListAsync();

        var result = new List<GetSensorsDto>();

        foreach (var sensor in sensors)
        {
            var sensorValue = distinctSensors.FirstOrDefault(s => s!.SensorId == sensor.Id);
            if (sensorValue == null)
                continue;

            var types = new List<ValueType>();
            if (sensorValue.Humidity != 0)
                types.Add(ValueType.Humidity);
            if (sensorValue.Pm10 != 0)
                types.Add(ValueType.Pm10);
            if (sensorValue.Pm25 != 0)
                types.Add(ValueType.Pm25);
            if (sensorValue.Pressure != 0)
                types.Add(ValueType.Pressure);
            if (sensorValue.Temperature != 0)
                types.Add(ValueType.Temp);

            result.AddRange(types.Select(type => new GetSensorsDto()
                {Name = sensor.Location, UniqueId = sensor.Number, Type = type}));
        }

        return result;
    }

    public async Task FetchData(DateTime from, DateTime to, string? uniqueId)
    {
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .Where(s => s.Module.Source == Source.CykloKoalicia);

        if (!string.IsNullOrEmpty(uniqueId))
            query = query.Where(s => s.Module.UniqueId == uniqueId);

        var sensors = await query.ToListAsync();

        var sensorIds = sensors.Select(s => uint.Parse(s.UniqueId))
            .Distinct()
            .ToList();


        foreach (var sensorId in sensorIds)
        {
            var totalValues = await _ckVzduchContext.SensorsValues
                .Where(s => s.SensorId == sensorId && s.CreatedAt > from && s.CreatedAt < to)
                .CountAsync();

            var skip = 0;
            var take = 10000;
            while (true)
            {
                var sensorsValuesPart = await _ckVzduchContext.SensorsValues
                    .Where(s => s.SensorId == sensorId && s.CreatedAt > from && s.CreatedAt < to)
                    .OrderBy(s => s.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                var readings = new List<Reading>();
                foreach (var value in sensorsValuesPart)
                {
                    foreach (var sensor in sensors)
                    {
                        var datetime = value.CreatedAt?.ToUniversalTime() ?? DateTime.UtcNow;

                        switch (sensor.Type)
                        {
                            case ValueType.Humidity:
                                readings.Add(CreateReadingObject(sensor, value.Humidity, datetime));
                                break;
                            case ValueType.Pressure:
                                if (value.Pressure == 0)
                                    continue;
                                readings.Add(CreateReadingObject(sensor, value.Pressure, datetime));
                                break;
                            case ValueType.Temp:
                                if (value.Temperature is < -100 or > 100)
                                    continue;
                                readings.Add(CreateReadingObject(sensor, value.Temperature, datetime));
                                break;
                            case ValueType.Pm10:
                                readings.Add(CreateReadingObject(sensor, value.Pm10, datetime));
                                break;
                            case ValueType.Pm25:
                                readings.Add(CreateReadingObject(sensor, value.Pm25, datetime));
                                break;
                        }
                    }
                }

                var readingsByDay = readings
                    .GroupBy(r => new {r.Sensor, r.DateTime.Date})
                    .Select(g => new {g.Key.Sensor, g.Key.Date, Readings = g.ToList()})
                    .ToList();

                foreach (var readingsDay in readingsByDay)
                {
                    var isReadingsDayInDb = await _bpContext.Reading
                        .AnyAsync(r => r.SensorId == readingsDay.Sensor.Id && r.DateTime == readingsDay.Readings[0].DateTime);

                    if (isReadingsDayInDb)
                    {
                        _logger.LogInformation(
                            "CykloKoaliciaService: Data for sensor {SensorUniqueId} for day {Date} already in db",
                            sensorId, readingsDay.Date);
                        continue;
                    }

                    await _bpContext.Reading.AddRangeAsync(readingsDay.Readings);
                    _logger.LogInformation(
                        "CykloKoaliciaService: Data ({ReadingsCount}) for sensor {SensorUniqueId} for day {Date} added to db",
                        readingsDay.Readings.Count, sensorId, readingsDay.Date);
                }

                await _bpContext.SaveChangesAsync();

                if (sensorsValuesPart.Count < take)
                    break;

                skip += take;

                _logger.LogInformation(
                    "CykloKoaliciaService: Fetched data for sensor {SensorUniqueId}, {ValuesCount} values of {TotalValues}",
                    sensorId, skip, totalValues);
            }


            _logger.LogInformation("CykloKoaliciaService: Fetched data for sensor {SensorUniqueId}", sensorId);
        }
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

    private Reading CreateReadingObject(Sensor sensor, decimal value, DateTime dateTime)
    {
        return new Reading()
        {
            Sensor = sensor,
            Value = value,
            DateTime = dateTime,
        };
    }
}