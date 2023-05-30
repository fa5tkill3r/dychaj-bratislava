﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using BP.Data;
using BP.Data.DbModels;
using BP.Data.Dto.Request;
using BP.Data.Dto.Response;
using BP.Data.Dto.Response.Stats;
using Microsoft.EntityFrameworkCore;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Services;

public class Pm25Service
{
    private readonly BpContext _bpContext;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory;

    public Pm25Service(BpContext bpContext, IMapper mapper, IServiceScopeFactory scopeFactory)
    {
        _bpContext = bpContext;
        _mapper = mapper;
        _scopeFactory = scopeFactory;
    }

    public async Task<PM25StatsResponse> GetStats(StatsRequest? request)
    {
        var moduleIds = request?.Modules;
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25);


        if (moduleIds != null && moduleIds.Any())
        {
            var ids = moduleIds;
            query = query.Where(s => ids.Contains(s.Module.Id));
        }
        else
            query = query.Take(1);

        var sensors = await query.ToListAsync();

        var response = new PM25StatsResponse();

        foreach (var sensor in sensors)
        {
            var yearValueAvg = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id && r.DateTime.Date.Year == DateTime.UtcNow.Date.Year)
                .Average(r => (decimal?) r.Value);

            var dayValueAvg = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id && r.DateTime.Date == DateTime.UtcNow.Date)
                .Average(r => (decimal?) r.Value);

            var current = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id)
                .Where(r => r.DateTime.Date == DateTime.UtcNow.Date)
                .OrderByDescending(r => r.DateTime)
                .Select(r => (decimal?) r.Value)
                .FirstOrDefault();

            var daysAboveThreshold = await _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id && r.DateTime.Date.Year == DateTime.UtcNow.Date.Year)
                .GroupBy(r => r.DateTime.Date)
                .Where(g => g.Average(r => r.Value) > 25)
                .CountAsync();


            response.Modules.Add(new PM25StatsResponseModule()
            {
                YearValueAvg = yearValueAvg != null ? Math.Round(yearValueAvg.Value, 2) : null,
                DayValueAvg = dayValueAvg != null ? Math.Round(dayValueAvg.Value, 2) : null,
                Current = current != null ? Math.Round(current.Value, 2) : null,
                Module = _mapper.Map<ModuleDto>(sensor.Module),
                DaysAboveThreshold = daysAboveThreshold,
            });
        }


        var availableModules = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25)
            .ProjectTo<ModuleDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        response.AvailableModules = availableModules;


        return response;
    }

    public async Task<List<ModuleDto>> GetLocations()
    {
        var locations = await _bpContext.Sensor.Where(s => s.Type == ValueType.Pm25)
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Distinct()
            .ToListAsync();
        return _mapper.Map<List<ModuleDto>>(locations);
    }

    public async Task<Pm25WeeklyComparisonResponse> GetWeeklyComparison(Pm25WeeklyComparisonRequest? request)
    {
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25);

        if (request?.Modules != null && request.Modules.Any())
        {
            var ids = request.Modules;
            query = query.Where(s => ids.Contains(s.Module.Id));
        }
        else
            query = query.Take(3);

        var sensors = await query.ToListAsync();

        var response = new Pm25WeeklyComparisonResponse();

        var from = DateTime.UtcNow.Date.AddDays(-365);

        var fetchSensor = new Func<Sensor, Task>(async sensor =>
        {
            var readings = await FetchSensor(sensor, from, DateTime.UtcNow.Date);
            var module = _mapper.Map<ModuleWithReadingsDto>(sensor.Module);
            module.Readings = readings;

            response.Modules.Add(module);
        });

        var tasks = sensors.Select(sensor => fetchSensor(sensor)).ToList();

        await Task.WhenAll(tasks);

        var availableModules = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25)
            .ProjectTo<ModuleDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        response.AvailableModules = availableModules;

        return response;
    }

    private async Task<List<ReadingDto>> FetchSensor(Sensor sensor, DateTime from, DateTime to)
    {
        var tasks = new List<Task<ReadingDto>>();

        for (var date = from; date <= to; date = date.AddDays(7))
        {
            tasks.Add(FetchWeeklyReadings(sensor, date, date.AddDays(7)));
        }

        var readings = await Task.WhenAll(tasks);

        return readings.ToList();
    }

    private async Task<ReadingDto> FetchWeeklyReadings(Sensor sensor, DateTime from, DateTime to)
    {
        using var scope = _scopeFactory.CreateScope();
        var bpContext = scope.ServiceProvider.GetRequiredService<BpContext>();

        var avg = await bpContext.Reading
            .Where(r => r.SensorId == sensor.Id)
            .Where(r => r.DateTime.Date >= from.Date && r.DateTime.Date <= to.Date)
            .AverageAsync(r => (decimal?) r.Value);

        await bpContext.DisposeAsync();

        return new ReadingDto()
        {
            Value = avg != null ? Math.Round(avg.Value, 2) : null,
            DateTime = from,
        };
    }

    public async Task<object> GetMap()
    {
        var query = await  (
            from s in _bpContext.Sensor
            join m in _bpContext.Module on s.ModuleId equals m.Id
            join l in _bpContext.Location on m.LocationId equals l.Id into locations
            from l in locations.DefaultIfEmpty()
            join r in _bpContext.Reading on s.Id equals r.SensorId into readings
            from r in readings
                .Where(r => r.DateTime == _bpContext.Reading
                    .Where(r => r.SensorId == s.Id)
                    .Max(r => r.DateTime))
                .DefaultIfEmpty()
            where s.Type == ValueType.Pm25
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

        var response = new List<ModuleWithReadingsDto>();

        foreach (var sensor in sensors)
        {
            response.Add(new ModuleWithReadingsDto()
            {
                Id = sensor.Module.Id,
                Name = sensor.Module.Name,
                Location = _mapper.Map<LocationDto>(sensor.Module.Location),
                Readings = _mapper.Map<List<ReadingDto>>(sensor.Readings),
            });
        }


        return response;
    }

    public async Task<List<Pm25ExceedResponse>> GetYearlyExceed()
    {
        var response = new List<Pm25ExceedResponse>();
        
        var sensorsWithExceed = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25)
            .Select(s => new
            {
                Sensor = s,
                Exceed = _bpContext.Reading
                    .Where(r => r.SensorId == s.Id && r.DateTime.Date >= DateTime.UtcNow.Date.AddDays(-365))
                    .GroupBy(r => r.DateTime.Date)
                    .Count(g => g.Average(r => r.Value) > 25),
            })
            .ToListAsync();

        foreach (var sensorWithExceed in sensorsWithExceed)
        {
            response.Add(new Pm25ExceedResponse()
            {
                Module = _mapper.Map<ModuleDto>(sensorWithExceed.Sensor.Module),
                Exceed = sensorWithExceed.Exceed,
            });
        }
        
        response = response.OrderByDescending(r => r.Exceed).ToList();
        
        return response;
    }

    public async Task<List<ModuleWithReadingsDto>> GetCompare(Pm25CompareRequest request)
    {
        var sensors = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25)
            .Where(s => request.Modules.Contains(s.ModuleId))
            .ToListAsync();
        
        var response = new List<ModuleWithReadingsDto>();
        
        request.Hours = request.Hours.OrderBy(h => h).ToList();
        request.WeekDays = request.WeekDays.OrderBy(w => w).ToList();

        if (request.Weeks > 3)
            throw new Exception("Max 3 weeks allowed");

        

        foreach (var sensor in sensors)
        {
            var start = DateTime.UtcNow.Date.AddDays(request.Weeks * -7);
            var module = _mapper.Map<ModuleWithReadingsDto>(sensor.Module);
            module.Readings = new List<ReadingDto>();
            
            for (int i = 0; i < request.Weeks; i++)
            {
                foreach (var dayOfWeek in request.WeekDays)
                {
                    while (start.DayOfWeek != dayOfWeek)
                    {
                        start = start.AddDays(1);
                    }


                    var lastReading = DateTime.MinValue;
                    foreach (var hour in request.Hours)
                    {
                        if (lastReading != DateTime.MinValue && hour - lastReading.Hour > 1)
                        {
                            module.Readings.Add(new ReadingDto()
                            {
                                DateTime = lastReading,
                                Value = null,
                            });
                        }
                        
                        var from = new DateTime(start.Year, start.Month, start.Day, hour, 0, 0);
                        
                        var readings = await _bpContext.Reading
                            .Where(r => r.SensorId == sensor.Id)
                            .Where(r => r.DateTime >= from && r.DateTime < from.AddHours(1))
                            .OrderBy(r => r.DateTime)
                            .ToListAsync();

                        for (var startMinute = 0; startMinute < 60; startMinute += 10)
                        {
                            var endMinute = startMinute + 10;
                            var avg = readings
                                .Where(r => r.DateTime.Minute >= startMinute && r.DateTime.Minute < endMinute)
                                .Average(r => (decimal?) r.Value);

                            module.Readings.Add(new ReadingDto()
                            {
                                DateTime = from.AddMinutes(startMinute),
                                Value = avg != null ? Math.Round(avg.Value, 2) : null,
                            });
                            
                            lastReading = from.AddMinutes(startMinute);
                        }
                    }
                    
                    module.Readings.Add(new ReadingDto()
                    {
                        DateTime = lastReading + TimeSpan.FromMilliseconds(1),
                        Value = null,
                    });
                    
                    start = start.AddDays(1);
                }
            }
            
            module.Readings = module.Readings.OrderBy(r => r.DateTime).ToList();
            
            response.Add(module);
        }


        return response;
    }
}