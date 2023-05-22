using AutoMapper;
using BP.Data;
using BP.Data.DbModels;
using BP.Data.Dto;
using BP.Data.Dto.Request;
using Microsoft.EntityFrameworkCore;
using ValueType = BP.Data.Models.ValueType;

namespace BP.API.Services;

public class PM25Service
{
    private readonly BpContext _bpContext;
    private readonly IMapper _mapper;

    public PM25Service(BpContext _bpContext, IMapper _mapper)
    {
        this._bpContext = _bpContext;
        this._mapper = _mapper;
    }

    public async Task GetStats(PM25StatsRequest request)
    {
        var sensorId = request.SensorId;
        var sensor = await _bpContext.Sensor
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Where(s => s.Type == ValueType.PM25)
            .FirstOrDefaultAsync(s => s.Id == sensorId);
        if (sensor == null)
            sensor = await _bpContext.Sensor.FirstOrDefaultAsync(s => s.Type == ValueType.PM25);
        
        if (sensor == null)
            throw new Exception("No PM25 sensors found");

        Console.WriteLine(sensor.Module.Location.Name);
    }

    public async Task<List<LocationDto>> GetLocations()
    {
        var locations = await _bpContext.Sensor.Where(s => s.Type == ValueType.PM25)
            .Include(s => s.Module)
            .ThenInclude(m => m.Location)
            .Select(s => s.Module.Location)
            .Distinct()
            .ToListAsync();
        return _mapper.Map<List<LocationDto>>(locations);
    }
}