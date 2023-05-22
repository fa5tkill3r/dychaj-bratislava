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

    public Worker(BpContext bpContext, SensorCommunityService sensorCommunity)
    {
        _bpContext = bpContext;
        _sensorCommunity = sensorCommunity;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // await AddModule();
        await GetValue();
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