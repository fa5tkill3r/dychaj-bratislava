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
        
        var sensorCommunity = new WeatherWorker<SensorCommunityService>(_sensorCommunity, _bpContext);
        var shmuWeatherService = new WeatherWorker<ShmuWeatherService>(_shmuWeatherService, _bpContext);
        var shmuAirService = new WeatherWorker<ShmuAirService>(_shmuAirService, _bpContext);
        var cykloKoaliciaService = new WeatherWorker<CykloKoaliciaService>(_cykloKoaliciaService, _bpContext);
        
        // await sensorCommunity.AddModule("3", "24739", "24740");
        // await sensorCommunity.AddModule("4", "80379", "80380");
        // await sensorCommunity.AddModule("5", "12308", "12309");
        
        // await shmuAirService.AddModule("99112");
        // await shmuAirService.AddModule("11813");
        // await shmuAirService.AddModule("99111");
        // await shmuAirService.AddModule("99117");
        //
        // await shmuWeatherService.AddModule("11813");
        // await shmuWeatherService.AddModule("11816");
        // await shmuWeatherService.AddModule("11810");
        //
        // await cykloKoaliciaService.AddModule("1844542");
        // await cykloKoaliciaService.AddModule("7963083");
        // await cykloKoaliciaService.AddModule("7236237");
    }
}