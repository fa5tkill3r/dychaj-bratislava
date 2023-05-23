using AutoMapper;
using AutoMapper.QueryableExtensions;
using BP.Data;
using BP.Data.Dto.Request;
using BP.Data.Dto.Response;
using BP.Data.Dto.Response.Stats;
using Microsoft.EntityFrameworkCore;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Services;

public class PM25Service
{
    private readonly BpContext _bpContext;
    private readonly IMapper _mapper;

    public PM25Service(BpContext bpContext, IMapper mapper)
    {
        _bpContext = bpContext;
        _mapper = mapper;
    }

    public async Task<PM25StatsResponse> GetStats(PM25StatsRequest request)
    {
        var sensorId = request.SensorId;
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25)
            .SelectMany(s => s.Readings);

        if (sensorId != null)
            query = query.Where(s => s.SensorId == sensorId);
        var readings = await query.ToListAsync();

        
        var locations = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25)
            .ProjectTo<ModuleDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var stats = new PM25StatsResponse
        {
            YearValueAvg = readings
                .Where(r => r.DateTime.Date >= DateTime.UtcNow.Date.AddDays(-365))
                .Average(r => r.Value),
            DayValueAvg = readings
                .Where(r => r.DateTime.Date >= DateTime.UtcNow.Date.AddDays(-1))
                .Average(r => r.Value),
            Current = readings.MaxBy(r => r.DateTime)?.Value ?? 0,
            Modules = locations
        };

        return stats;
    }

    public async Task<List<LocationDto>> GetLocations()
    {
        var locations = await _bpContext.Sensor.Where(s => s.Type == ValueType.Pm25)
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Select(s => s.Module.Location)
            .Distinct()
            .ToListAsync();
        return _mapper.Map<List<LocationDto>>(locations);
    }
}