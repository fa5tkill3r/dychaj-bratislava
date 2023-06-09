using BP.API.Services.WeatherServices;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var bpContext = scope.ServiceProvider.GetRequiredService<BpContext>();
        var sensorCommunity = scope.ServiceProvider.GetRequiredService<SensorCommunityService>();
        var shmuWeatherService = scope.ServiceProvider.GetRequiredService<ShmuWeatherService>();
        var shmuAirService = scope.ServiceProvider.GetRequiredService<ShmuAirService>();
        var cykloKoaliciaService = scope.ServiceProvider.GetRequiredService<CykloKoaliciaService>();
        var wsensorCommunity = new WeatherWorker<SensorCommunityService>(sensorCommunity, bpContext);
        var wshmuWeatherService = new WeatherWorker<ShmuWeatherService>(shmuWeatherService, bpContext);
        var wshmuAirService = new WeatherWorker<ShmuAirService>(shmuAirService, bpContext);
        var wcykloKoaliciaService = new WeatherWorker<CykloKoaliciaService>(cykloKoaliciaService, bpContext);
            
        await wsensorCommunity.AddModule("Meno Modulu", "1234", "4564");
    }
}