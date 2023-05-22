using BP.API.Services;
using BP.Data;
using BP.Data.CykloKoalicia;
using BP.DataFetcher;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// hosting context

// Add configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();


// Add services to the container.
// builder.Services.AddScoped<ValueService>();
builder.Services.AddScoped<SensorCommunityService>();
builder.Services.AddScoped<ShmuService>();
builder.Services.AddScoped<GoogleService>();



builder.Services.AddDbContext<BpContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cyklokoaliciaConnection = builder.Configuration.GetConnectionString("Cyklokoalicia");

if (cyklokoaliciaConnection != null)
{
    builder.Services.AddDbContext<CkVzduchContext>(options =>
        options.UseMySQL(cyklokoaliciaConnection));
}

builder.Services.AddHostedService<DataFetcher>();

var app = builder.Build();
app.Run();