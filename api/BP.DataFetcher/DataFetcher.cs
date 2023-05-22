using BP.API.Services;

namespace BP.DataFetcher;

public class DataFetcher : BackgroundService
{
    private readonly ILogger<DataFetcher> _logger;
    private readonly SensorCommunityService _sensorCommunityService;

    public DataFetcher(ILogger<DataFetcher> logger, SensorCommunityService sensorCommunityService)
    {
        _logger = logger;
        _sensorCommunityService = sensorCommunityService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            _ = _sensorCommunityService.GetData().ConfigureAwait(false);
            
            
            
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}