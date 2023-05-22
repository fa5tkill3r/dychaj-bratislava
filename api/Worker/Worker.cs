using BP.API.Services;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using Microsoft.Extensions.Hosting;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly BpContext _bpContext;
    private readonly SensorCommunityService _sensorCommunity;
    private readonly ShmuWeatherService _shmuWeatherService;
    private readonly ShmuAirService _shmuAirService;

    public Worker(BpContext bpContext, SensorCommunityService sensorCommunity, ShmuWeatherService shmuWeatherService,
        ShmuAirService shmuAirService)
    {
        _bpContext = bpContext;
        _sensorCommunity = sensorCommunity;
        _shmuWeatherService = shmuWeatherService;
        _shmuAirService = shmuAirService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // await AddModule();
        // await GetValue();

        // await AddShmuAirModule("99112");

        // await _shmuAirService.GetData();
        
        var weatherService = new WeatherWorker<ShmuWeatherService>(_shmuWeatherService, _bpContext);
        await weatherService.AddModule("11813");
        await weatherService.GetData();
    }



    private async Task AddModule()
    {
        var module = new Module()
        {
            Name = "SensorCommunity Test",
            Source = Source.SensorCommunity,
        };
        await _bpContext.Module.AddAsync(module);

        await _sensorCommunity.AddSensor(module, "50737");
    }

    private async Task GetValue()
    {
        await _sensorCommunity.GetData();
    }
}