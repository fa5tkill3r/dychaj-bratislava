using BP.API.Services.WeatherServices;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models.Shmu;
using Microsoft.Extensions.Hosting;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly BpContext _bpContext;
    private readonly SensorCommunityService _sensorCommunity;
    private readonly ShmuAirService _shmuAirService;
    private readonly CykloKoaliciaService _cykloKoaliciaService;
    private readonly ShmuWeatherService _shmuWeatherService;

    public Worker(BpContext bpContext, SensorCommunityService sensorCommunity, ShmuWeatherService shmuWeatherService,
        ShmuAirService shmuAirService, CykloKoaliciaService cykloKoaliciaService)
    {
        _bpContext = bpContext;
        _sensorCommunity = sensorCommunity;
        _shmuWeatherService = shmuWeatherService;
        _shmuAirService = shmuAirService;
        _cykloKoaliciaService = cykloKoaliciaService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // var weatherService = new WeatherWorker<CykloKoaliciaService>(_cykloKoaliciaService, _bpContext);
        // await weatherService.AddModule("1844542");
        // await weatherService.GetData();
        
        var weatherService = new WeatherWorker<ShmuWeatherService>(_shmuWeatherService, _bpContext);
        await weatherService.GetData();
    }
}