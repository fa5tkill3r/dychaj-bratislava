using BP.API.Services;
using BP.Data;
using BP.Data.TestModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


// Add configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();


// Add services to the container.
// builder.Services.AddScoped<ValueService>();
// builder.Services.AddScoped<SensorCommunityService>();


builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cyklokoaliciaConnection = builder.Configuration.GetConnectionString("Cyklokoalicia");

if (cyklokoaliciaConnection != null)
{
    builder.Services.AddDbContext<TestContext>(options =>
        options.UseMySQL(cyklokoaliciaConnection));
}

builder.Services.AddHostedService<Worker.Worker>();

var app = builder.Build();



app.Run();