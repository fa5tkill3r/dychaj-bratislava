using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models;
using BP.Data.Models.SensorCommunity;
using Microsoft.EntityFrameworkCore;
using Location = BP.Data.DbModels.Location;
using Sensor = BP.Data.DbModels.Sensor;
using ValueType = BP.Data.DbHelpers.ValueType;

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
                    $"https://data.sensor.community/airrohr/v1/sensor/{uniqueId}/", 10);
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
        
        /*
         * This is a workaround for the fact that the API only allows 2 request per second.
         * In case that some other function calls same endpoint, we always have one request left.
         * Otherwise we would get error 500.
         */
        foreach (var uniqueId in uniqueIds)
        {
            await fetchData(uniqueId);
        }
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

                        module.Name = loc.StreetName;
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

        return sensors.Select(sensor => new GetSensorsDto()
        {
            Name = sensor.id.ToString(), UniqueId = sensor.id.ToString(),
            Type = Helpers.GetTypeFromString(sensor.sensordatavalues[0].value_type),
        }).ToList();
    }

    public async Task FetchData(DateTime from, DateTime to, string? uniqueId)
    {
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .Where(s => s.Module.Source == Source.SensorCommunity);
        if (!string.IsNullOrEmpty(uniqueId))
            query = query.Where(s => s.UniqueId == uniqueId);
        var sensorUniqueIds = await query
            .Select(s => s.UniqueId)
            .Distinct()
            .ToListAsync();
        var fetchData = new Func<string, Task>(async sensorUniqueId =>
        {
            _logger.LogInformation("SensorCommunityService: Getting data for sensor {SensorUniqueId}", sensorUniqueId);

            var response =
                await Requests.Get<List<SensorCommunity>>(
                    $"https://data.sensor.community/airrohr/v1/sensor/{sensorUniqueId}/");
            var sensorName = response?.FirstOrDefault()?.sensor.sensor_type.name;

            if (sensorName == null)
            {
                _logger.LogError("SensorCommunityService: Failed to get data for sensor {SensorUniqueId}",
                    sensorUniqueId);
                return;
            }
            
            var semaphore = new SemaphoreSlim(50);
            var dayFetchTasks = new List<Task>();
            for (var date = from.Date; date.Date <= to.Date; date = date.AddDays(1))
            {
                await semaphore.WaitAsync();
                dayFetchTasks.Add(FetchDayTask(sensorUniqueId, sensorName, date)
                    .ContinueWith((task) =>
                {
                    dayFetchTasks.Remove(task);
                    semaphore.Release();
                }));
                
                dayFetchTasks.RemoveAll(t => t.IsCompleted);
            }
            
            await Task.WhenAll(dayFetchTasks);
            
            _logger.LogInformation("SensorCommunityService: Finished getting data for sensor {SensorUniqueId}",
                sensorUniqueId);
        });

        var tasks = sensorUniqueIds.Select(sensor => fetchData(sensor)).ToList();
        
        foreach (var task in tasks)
        {
            await task;
        }   
        
        // await Task.WhenAll(tasks);
    }

    private async Task FetchDayTask(string sensorUniqueId, string sensorName, DateTime fetchDate)
    {
        var scope = _scopeFactory.CreateScope();
        var bpContext = scope.ServiceProvider.GetRequiredService<BpContext>();
        
        var isReadingInDb = await bpContext.Reading
            .Include(r => r.Sensor)
            .AnyAsync(r =>
                r.Sensor.UniqueId == sensorUniqueId && r.DateTime.Date == fetchDate.Date);
        if (isReadingInDb)
            return;

        var url = "https://archive.sensor.community/";

        if (fetchDate.Year == DateTime.Now.Year)
            url += $"{fetchDate.Year}-{fetchDate.Month:D2}-{fetchDate.Day:D2}/";
        else
            url += $"{fetchDate.Year}/{fetchDate.Year}-{fetchDate.Month:D2}-{fetchDate.Day:D2}/";
        url +=
            $"{fetchDate.Year}-{fetchDate.Month:D2}-{fetchDate.Day:D2}_{sensorName.ToLower()}_sensor_{sensorUniqueId}.csv";


        var csv = await Requests.GetFileStream(url);

        if (csv == null)
        {
            _logger.LogWarning(
                "SensorCommunityService: Failed to get data for sensor {SensorUniqueId} on {Date}",
                sensorUniqueId, fetchDate);
            return;
        }

        using var reader = new StreamReader(csv);


        var valueTypes = new List<Tuple<ValueType, int>>();
        var lines = (await reader.ReadToEndAsync()).Split("\n");

        var readingInterval = TimeSpan.FromMinutes(5); 
        var lastReading = DateTime.MinValue;
        var readingsCount = 0;
        foreach (var (line, index) in lines.WithIndex())
        {
            if (string.IsNullOrEmpty(line))
                continue;

            var columns = line.Split(";");
            if (index == 0)
            {
                foreach (var (column, columnIndex) in columns.WithIndex())
                {
                    if (Helpers.TryGetTypeFromString(column, out var type))
                        valueTypes.Add(new Tuple<ValueType, int>(type, columnIndex));
                }

                continue;
            }

            if (valueTypes.Count == 0)
            {
                _logger.LogError(
                    "SensorCommunityService: Failed to find any data for sensor {SensorUniqueId} on {Date}",
                    sensorUniqueId, fetchDate);
                continue;
            }

            var time = DateTime.Parse(columns[5]);
            
            if (time - lastReading < readingInterval)
                continue;
            
            var sensors = await bpContext.Sensor
                .Include(s => s.Module)
                .Where(s => s.Module.Source == Source.SensorCommunity)
                .Where(s => s.UniqueId == sensorUniqueId)
                .ToListAsync();

            foreach (var (valueType, columnIndex) in valueTypes)
            {
                var sensor = sensors.FirstOrDefault(s => s.Type == valueType);

                if (sensor == null)
                {
                    _logger.LogError(
                        "SensorCommunityService: Failed to find sensor {SensorUniqueId} of type {Type} on {Date}",
                        sensorUniqueId, valueType, fetchDate);
                    continue;
                }
                
                if (!decimal.TryParse(columns[columnIndex], out var value))
                {
                    _logger.LogWarning(
                        "SensorCommunityService: Failed to parse value for sensor {SensorUniqueId} of type {Type} on {Date}",
                        sensorUniqueId, valueType, time);
                    continue;
                }

                var reading = new Reading
                {
                    SensorId = sensor.Id,
                    DateTime = time,
                    Value = value,
                };
                await bpContext.Reading.AddAsync(reading);
                
                lastReading = time;
                readingsCount++;

                _logger.LogTrace(
                    "SensorCommunityService: Added reading for sensor {SensorUniqueId} of type {Type} on {Date}",
                    sensorUniqueId, valueType, fetchDate);
            }
        }
        
        _logger.LogInformation(
            "SensorCommunityService: Finished getting data for sensor {SensorUniqueId} on {Date}. Added {ReadingsCount} readings",
            sensorUniqueId, fetchDate, readingsCount);

        await csv.DisposeAsync();
        
        await bpContext.SaveChangesAsync();
    }
}