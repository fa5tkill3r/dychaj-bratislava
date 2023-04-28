using BP.API.Services;
using BP.Data;
using BP.Data.TestModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Worker;

public class Worker : BackgroundService
{
    private readonly TestContext _cyklokoaliciaContext;

    public Worker(TestContext cyklokoaliciaContext)
    {
        _cyklokoaliciaContext = cyklokoaliciaContext;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Hello World!");

        var x = await _cyklokoaliciaContext.TableNames.ToListAsync();
        Console.WriteLine(x.Count);
    }
}