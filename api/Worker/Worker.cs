using BP.API.Services;
using Microsoft.Extensions.Hosting;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly SensorCommunityService _sensorCommunityService;

    public Worker(SensorCommunityService sensorCommunityService)
    {
        _sensorCommunityService = sensorCommunityService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Hello World!");
        await _sensorCommunityService.GetData();
    }
}