using BP.Data.CykloKoalicia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly CkVzduchContext _cyklokoaliciaContext;

    public Worker(CkVzduchContext cyklokoaliciaContext)
    {
        _cyklokoaliciaContext = cyklokoaliciaContext;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Hello World!");

        var x = await _cyklokoaliciaContext.Sensors.ToListAsync(stoppingToken);
        Console.WriteLine(x.Count);
    }
}