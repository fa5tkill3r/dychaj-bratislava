using BP.API.Services.WeatherServices;
using BP.Data;
using BP.Data.DbModels;
using BP.Data.Models;

namespace Worker;

public class WeatherWorker<T> where T : IWeatherService
{
    private readonly BpContext _bpContext;
    private readonly T _tWeatherService;

    public WeatherWorker(T tWeatherService, BpContext bpContext)
    {
        _tWeatherService = tWeatherService;
        _bpContext = bpContext;
    }

    public async Task AddModule(string moduleName, params string[] uniqueIds)
    {
        var module = new Module
        {
            Name = moduleName,
        };
        await _bpContext.Module.AddAsync(module);
        await _bpContext.SaveChangesAsync();
        try
        {
            foreach (var id in uniqueIds)
            {
                await _tWeatherService.AddSensor(module, id);
            }
        }
        catch (Exception e)
        {
            _bpContext.Module.Remove(module);
            await _bpContext.SaveChangesAsync();
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task GetData()
    {
        await _tWeatherService.GetData();
    }
    
    public async Task<List<GetSensorsDto>> GetSensors()
    {
        return await _tWeatherService.GetSensors();
    }
    
    public async Task FetchData(DateTime from, DateTime to, string? uniqueId)
    {
        await _tWeatherService.FetchData(from, to, uniqueId);
    }
}