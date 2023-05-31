using AutoMapper;
using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Dto.Request;
using BP.Data.Dto.Response;
using BP.Data.Dto.Response.Stats;
using Microsoft.EntityFrameworkCore;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Services;

public class BasicService
{
    private readonly BpContext _bpContext;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory;

    public BasicService(BpContext bpContext, IMapper mapper, IServiceScopeFactory scopeFactory)
    {
        _bpContext = bpContext;
        _mapper = mapper;
        _scopeFactory = scopeFactory;
    }

    public async Task<List<SensorDto>> GetLocations(ValueType valueType)
    {
        var sensors = await _bpContext.Sensor.Where(s => s.Type == valueType)
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Distinct()
            .ToListAsync();
        return _mapper.Map<List<SensorDto>>(sensors);
    }

    public async Task<BasicStatsResponse> GetStats(ValueType valueType, StatsRequest? request)
    {
        var sensorIds = request?.Sensors;
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == valueType);


        if (sensorIds != null && sensorIds.Any())
        {
            var ids = sensorIds;
            query = query.Where(s => ids.Contains(s.Id));
        }
        else
            query = query.Take(1);

        var sensors = await query.ToListAsync();

        var response = new BasicStatsResponse();

        foreach (var sensor in sensors)
        {
            var max = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id && r.DateTime.Date == DateTime.UtcNow.Date)
                .Max(r => (decimal?) r.Value);
            
            var min = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id && r.DateTime.Date == DateTime.UtcNow.Date)
                .Min(r => (decimal?) r.Value);

            var current = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id)
                .Where(r => r.DateTime.Date == DateTime.UtcNow.Date)
                .OrderByDescending(r => r.DateTime)
                .Select(r => (decimal?) r.Value)
                .FirstOrDefault();

            response.Sensors.Add(new BasicStatSensor()
            {
                Max =  max != null ? Math.Round(max.Value, 2) : null,
                Min = min != null ? Math.Round(min.Value, 2) : null,
                Current = current != null ? Math.Round(current.Value, 2) : null,
                Sensor = _mapper.Map<SensorDto>(sensor),
            });
        }

        return response;
    }

    public async Task<BasicDataResponse> GetData(ValueType valueType, GetDataRequest? request)
    {
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == valueType);

        if (request?.Sensors != null && request.Sensors.Any())
        {
            var ids = request.Sensors;
            query = query.Where(s => ids.Contains(s.Id));
        }
        else
            query = query.Take(3);

        var sensors = await query.ToListAsync();

        var response = new BasicDataResponse();

        if (request == null)
            throw new Exception();
        

        var fetchSensor = new Func<Sensor, Task>(async sensor =>
        {
            var readings = await FetchSensor(sensor, request.From, request.To, request.Interval.ToDateTime());
            var sensorDto = _mapper.Map<SensorWithReadingsDto>(sensor);
            sensorDto.Readings = readings;

            response.Sensors.Add(sensorDto);
        });

        var tasks = sensors.Select(sensor => fetchSensor(sensor)).ToList();

        await Task.WhenAll(tasks);


        return response;
    }
    
    private async Task<List<ReadingDto>> FetchSensor(Sensor sensor, DateTimeOffset from, DateTimeOffset to, TimeSpan interval)
    {
        var tasks = new List<Task<ReadingDto>>();

        for (var date = from; date <= to; date = date.Add(interval))
        {
            tasks.Add(FetchWeeklyReadings(sensor, date, date.Add(interval)));
        }

        var readings = await Task.WhenAll(tasks);

        return readings.ToList();
    }

    private async Task<ReadingDto> FetchWeeklyReadings(Sensor sensor, DateTimeOffset from, DateTimeOffset to)
    {
        using var scope = _scopeFactory.CreateScope();
        var bpContext = scope.ServiceProvider.GetRequiredService<BpContext>();

        var avg = await bpContext.Reading
            .Where(r => r.SensorId == sensor.Id)
            .Where(r => r.DateTime >= from.ToUniversalTime().DateTime && r.DateTime <= to.ToUniversalTime().DateTime)
            .AverageAsync(r => (decimal?) r.Value);

        await bpContext.DisposeAsync();

        return new ReadingDto()
        {
            Value = avg != null ? Math.Round(avg.Value, 2) : null,
            DateTime = from,
        };
    }

    public async Task<List<SensorWithReadingsDto>> GetMap(ValueType valueType)
    {
        var query = await  (
            from s in _bpContext.Sensor
            join m in _bpContext.Module on s.ModuleId equals m.Id
            join l in _bpContext.Location on m.LocationId equals l.Id into locations
            from l in locations.DefaultIfEmpty()
            join r in _bpContext.Reading on s.Id equals r.SensorId into readings
            from r in readings
                .Where(r => r.DateTime == _bpContext.Reading
                    .Where(rr => rr.SensorId == s.Id)
                    .Max(rr => rr.DateTime))
                .DefaultIfEmpty()
            where s.Type == valueType
            select new
            {
                s,
                m,
                l,
                r
            }
        ).ToListAsync();
        
        foreach (var x1 in query)
        {
            x1.s.Module = x1.m;
            x1.s.Module.Location = x1.l;
            x1.s.Readings = new List<Reading>() {x1.r};
        }
        
        var sensors = query.Select(x => x.s).ToList();

        var response = new List<SensorWithReadingsDto>();

        foreach (var sensor in sensors)
        {
            response.Add(new SensorWithReadingsDto()
            {
                Id = sensor.Id,
                Name = sensor.GetName(),
                Description = sensor.Description,
                Type = sensor.Type.ToString(),
                Location = _mapper.Map<LocationDto>(sensor.Module.Location),
                Readings = _mapper.Map<List<ReadingDto>>(sensor.Readings),
                Module = _mapper.Map<ModuleDto>(sensor.Module),
            });
        }


        return response;
    }
}