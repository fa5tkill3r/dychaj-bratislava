using BP.API.Services;
using BP.API.Services.WeatherServices;
using BP.Data;
using BP.Data.CykloKoalicia;
using BP.DataFetcher;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// hosting context

// Add configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();


// Add services to the container.
// builder.Services.AddScoped<ValueService>();
builder.Services.AddScoped<IWeatherService, SensorCommunityService>();
builder.Services.AddScoped<IWeatherService, ShmuAirService>();
builder.Services.AddScoped<IWeatherService, ShmuWeatherService>();
builder.Services.AddScoped<GoogleService>();


builder.Services.AddDbContext<BpContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cyklokoaliciaConnection = builder.Configuration.GetConnectionString("Cyklokoalicia");

if (cyklokoaliciaConnection != null)
    builder.Services.AddDbContext<CkVzduchContext>(options =>
        options.UseMySQL(cyklokoaliciaConnection));

builder.Services.AddHostedService<DataFetcher>();

var app = builder.Build();
app.Run();