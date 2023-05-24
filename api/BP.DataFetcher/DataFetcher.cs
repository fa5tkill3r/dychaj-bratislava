using System.Collections;
using BP.API.Services.WeatherServices;

namespace BP.DataFetcher;

public class DataFetcher : BackgroundService
{
    private readonly ILogger<DataFetcher> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DataFetcher(ILogger<DataFetcher> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var weatherServices = scope.ServiceProvider.GetRequiredService<IEnumerable<IWeatherService>>();
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

                foreach (var weatherService in weatherServices) await GetData(weatherService);

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task GetData<T>(T service) where T : IWeatherService
    {
        try
        {
            await service.GetData();

            _logger.LogInformation("Data from {Service} successfully fetched", service.GetType().Name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting data from {Service} stacktrace: {StackTrace}",
                service.GetType().Name, e.StackTrace);
        }
    }
}