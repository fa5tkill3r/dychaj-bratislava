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
        var moduleIds = request.ModuleIds;
        var query = _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.Pm25);


        if (moduleIds != null)
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
                .Average(r => r.Value);
            
            var dayValueAvg = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id && r.DateTime.Date == DateTime.UtcNow.Date)
                .Average(r => r.Value);
            
            var current = _bpContext.Reading
                .Where(r => r.SensorId == sensor.Id)
                .OrderByDescending(r => r.DateTime)
                .Select(r => r.Value)
                .FirstOrDefault();
            
            response.Modules.Add(new PM25StatsResponseModule()
            {
                YearValueAvg = yearValueAvg,
                DayValueAvg = dayValueAvg,
                Current = current,
                Module = _mapper.Map<ModuleDto>(sensor.Module)
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