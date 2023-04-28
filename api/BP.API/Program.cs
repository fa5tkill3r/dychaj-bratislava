using BP.API.Services;
using BP.Data;
using BP.Data.TestModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// use urls
builder.WebHost.UseUrls("http://localhost:9000", "http://0.0.0.0:9000");

// Add configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();


// Add services to the container.
builder.Services.AddScoped<ValueService>();
builder.Services.AddScoped<GoogleService>();
builder.Services.AddScoped<SensorCommunityService>();
builder.Services.AddScoped<ShmuService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddDbContext<Context>();

builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cyklokoaliciaConnection = builder.Configuration.GetConnectionString("Cyklokoalicia");

if (cyklokoaliciaConnection != null)
{
    builder.Services.AddDbContext<TestContext>(options =>
        options.UseMySQL(cyklokoaliciaConnection));
}



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();