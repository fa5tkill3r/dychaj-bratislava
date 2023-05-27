using BP.API.Services;
using BP.API.Services.WeatherServices;
using BP.Data;
using BP.Data.CykloKoalicia;
using BP.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// use urls
builder.WebHost.UseUrls("http://0.0.0.0:9000");

// Add configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", true, true) // Load the production settings
    .AddJsonFile("appsettings.development.json", true, true) // Load the development settings
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true,
        true) // Load environment-specific settings
    .Build();


// Add services to the container.
builder.Services.AddScoped<ValueService>();
builder.Services.AddScoped<Pm25Service>();


builder.Services.AddScoped<GoogleService>();
builder.Services.AddScoped<SensorCommunityService>();
builder.Services.AddScoped<ShmuAirService>();

builder.Services.AddAutoMapper(typeof(MappingProfiles));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddDbContext<Context>();

builder.Services.AddDbContext<BpContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cyklokoaliciaConnection = builder.Configuration.GetConnectionString("Cyklokoalicia");

if (cyklokoaliciaConnection != null)
    builder.Services.AddDbContext<CkVzduchContext>(options =>
        options.UseMySQL(cyklokoaliciaConnection));


var corsPolicy = "CorsPolicy";

builder.Services.AddCors(options =>
{
#if DEBUG
    options.AddPolicy(corsPolicy,
        corsPolicyBuilder => corsPolicyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
#else
    options.AddPolicy("CorsPolicy",
        corsPolicyBuilder => corsPolicyBuilder
            .WithOrigins("https://air.masmute.net")
            .AllowAnyMethod()
            .AllowAnyHeader());
#endif
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseAuthorization();

app.MapControllers();

#if RELEASE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<BpContext>();    
    context.Database.Migrate();
}
#endif

app.Run();