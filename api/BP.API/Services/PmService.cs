using AutoMapper;
using BP.Data;
using BP.Data.Dto.Request;
using BP.Data.Dto.Response;
using BP.Data.Dto.Response.Stats;
using Microsoft.EntityFrameworkCore;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Services;

public class PmService
{
    private readonly BpContext _bpContext;
    private readonly IMapper _mapper;

    public PmService(BpContext bpContext, IMapper mapper)
    {
        _bpContext = bpContext;
        _mapper = mapper;
    }

    public async Task<PmStatsResponse> GetStats(ValueType valueType, StatsRequest? request)
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
            query = query.Where(s => s.Default);

        var sensors = await query.ToListAsync();

        var response = new PmStatsResponse();

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


            response.Sensors.Add(new PmStatsSensor()
            {
                YearValueAvg = yearValueAvg != null ? Math.Round(yearValueAvg.Value, 2) : null,
                DayValueAvg = dayValueAvg != null ? Math.Round(dayValueAvg.Value, 2) : null,
                Current = current != null ? Math.Round(current.Value, 2) : null,
                Sensor = _mapper.Map<SensorDto>(sensor),
                DaysAboveThreshold = daysAboveThreshold,
            });
        }

        return response;
    }

    public async Task<List<PmExceedResponse>> GetYearlyExceed(ValueType valueType)
    {
        var response = new List<PmExceedResponse>();
        
        var sensorsWithExceed = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == valueType)
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
            response.Add(new PmExceedResponse()
            {
                Sensor = _mapper.Map<SensorDto>(sensorWithExceed.Sensor),
                Exceed = sensorWithExceed.Exceed,
            });
        }
        
        response = response.OrderByDescending(r => r.Exceed).ToList();
        
        return response;
    }

    public async Task<List<SensorWithReadingsDto>> GetCompare(ValueType valueType, PmCompareRequest request)
    {
        var sensors = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == valueType)
            .Where(s => request.Sensors.Contains(s.Id))
            .ToListAsync();
        
        var response = new List<SensorWithReadingsDto>();
        
        request.Hours = request.Hours.OrderBy(h => h).ToList();
        request.WeekDays = request.WeekDays.OrderBy(w => w).ToList();

        if (request.Weeks > 3)
            throw new Exception("Max 3 weeks allowed");

        

        foreach (var sensor in sensors)
        {
            var start = DateTime.UtcNow.Date.AddDays(request.Weeks * -7);
            var sensorDto = _mapper.Map<SensorWithReadingsDto>(sensor);
            sensorDto.Readings = new List<ReadingDto>();
            
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
                            sensorDto.Readings.Add(new ReadingDto()
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

                            sensorDto.Readings.Add(new ReadingDto()
                            {
                                DateTime = from.AddMinutes(startMinute),
                                Value = avg != null ? Math.Round(avg.Value, 2) : null,
                            });
                            
                            lastReading = from.AddMinutes(startMinute);
                        }
                    }
                    
                    sensorDto.Readings.Add(new ReadingDto()
                    {
                        DateTime = lastReading + TimeSpan.FromMilliseconds(1),
                        Value = null,
                    });
                    
                    start = start.AddDays(1);
                }
            }
            
            sensorDto.Readings = sensorDto.Readings.OrderBy(r => r.DateTime).ToList();
            
            response.Add(sensorDto);
        }


        return response;
    }
}