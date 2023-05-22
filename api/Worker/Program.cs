using BP.API.Services;
using BP.API.Services.WeatherServices;
using BP.Data;
using BP.Data.CykloKoalicia;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


// Add configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();


// Add services to the container.
// builder.Services.AddScoped<ValueService>();
builder.Services.AddScoped<SensorCommunityService>();
builder.Services.AddScoped<ShmuAirService>();
builder.Services.AddScoped<ShmuWeatherService>();
builder.Services.AddScoped<CykloKoaliciaService>();
builder.Services.AddScoped<GoogleService>();


builder.Services.AddDbContext<BpContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cyklokoaliciaConnection = builder.Configuration.GetConnectionString("Cyklokoalicia");

if (cyklokoaliciaConnection != null)
    builder.Services.AddDbContext<CkVzduchContext>(options =>
        options.UseMySQL(cyklokoaliciaConnection));

builder.Services.AddHostedService<Worker.Worker>();

var app = builder.Build();


app.Run();