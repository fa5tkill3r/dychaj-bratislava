using BP.API.Services;
using BP.API.Services.WeatherServices;
using BP.Data;
using BP.Data.CykloKoalicia;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);


// Add configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile("appsettings.development.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .Build();



// Add services to the container.
builder.Services.AddHostedService<Worker.Worker>();
builder.Services.AddScoped<SensorCommunityService>();
builder.Services.AddScoped<ShmuAirService>();
builder.Services.AddScoped<ShmuWeatherService>();
builder.Services.AddScoped<CykloKoaliciaService>();
builder.Services.AddScoped<GoogleService>();

builder.Services.AddDbContext<BpContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cyklokoaliciaConnection = builder.Configuration.GetConnectionString("CykloKoalicia");

if (cyklokoaliciaConnection != null)
    builder.Services.AddDbContext<CkVzduchContext>(options =>
        options.UseMySQL(cyklokoaliciaConnection));



var app = builder.Build();

app.Run();