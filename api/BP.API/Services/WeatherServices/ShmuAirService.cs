﻿using System.Web;
using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models.Shmu;
using Microsoft.EntityFrameworkCore;

namespace BP.API.Services.WeatherServices;

public class ShmuAirService : IWeatherService
{
    private readonly string[] _allowedTypes =
    {
        "PM10",
        "PM2.5"
    };

    private readonly BpContext _bpContext;
    private readonly GoogleService _googleService;
    private readonly ILogger<ShmuAirService> _logger;

    public ShmuAirService(BpContext bpContext, ILogger<ShmuAirService> logger, GoogleService googleService)
    {
        _bpContext = bpContext;
        _logger = logger;
        _googleService = googleService;
    }

    public async Task GetData()
    {
        var modules = await _bpContext.Module
            .Where(m => m.Source == Source.ShmuAir)
            .Include(m => m.Sensors)
            .ToListAsync();

        var tasks = modules.Select(GetDataFromModule).ToList();

        await Task.WhenAll(tasks);
    }

    public async Task AddSensor(Module module, string uniqueId)
    {
        var shmuResponses = await Requests.Get<List<ShmuAirResponse>>(
            $"https://www.shmu.sk/api/v1/airquality/getdata?station={uniqueId}&history=1&station_meta=1");
        if (shmuResponses == null || !shmuResponses.Any())
        {
            _logger.LogError("ShmuService: Failed to get data");
            return;
        }

        var shmuResponse = shmuResponses.First();

        module.UniqueId = uniqueId;
        module.Source = Source.ShmuAir;
        module.Name = shmuResponse.station.station_name;


        await _bpContext.Entry(module).Reference(m => m.Location).LoadAsync();

        if (module.Location == null)
        {
            if (shmuResponse.station.gps_lat == null || shmuResponse.station.gps_lon == null)
            {
                _logger.LogInformation("ShmuService: Failed to location for station {StationId}", uniqueId);
            }
            else
            {
                var loc = await _googleService.GetLocation((double) shmuResponse.station.gps_lat,
                    (double) shmuResponse.station.gps_lon);

                if (loc == null)
                    _logger.LogInformation("ShmuService: Failed to get location for station {StationId}", uniqueId);
                else
                    module.Location = new Location
                    {
                        Address = loc.Address,
                        Name = loc.StreetName,
                        Latitude = loc.Latitude,
                        Longitude = loc.Longitude
                    };
            }
        }

        foreach (var stationPollutant in shmuResponse.station.pollutants)
        {
            if (!_allowedTypes.Contains(stationPollutant.pollutant_desc))
                continue;

            var sensor = new Sensor
            {
                Module = module,
                UniqueId = stationPollutant.pollutant_id,
                Description = stationPollutant.pollutant_desc,
                Type = Helpers.GetTypeFromString(stationPollutant.pollutant_desc),
                Name = stationPollutant.pollutant_desc
            };
            await _bpContext.AddAsync(sensor);
        }

        await _bpContext.SaveChangesAsync();
    }

    private async Task GetDataFromModule(Module module)
    {
        var shmuUriBuilder = new UriBuilder("https://www.shmu.sk/api/v1/airquality/getdata");

        var query = HttpUtility.ParseQueryString(shmuUriBuilder.Query);
        query["station"] = module.UniqueId;
        query["history"] = "1";

        shmuUriBuilder.Query = query.ToString();

        var station = await Requests.Get<List<ShmuAirResponse>>(shmuUriBuilder.ToString());

        if (station == null)
        {
            _logger.LogError("ShmuService: Failed to get data for station {StationId}", module.UniqueId);
            return;
        }

        foreach (var shmuResponse in station)
        foreach (var data in shmuResponse.data)
        {
            var sensor = _bpContext.Sensor.FirstOrDefault(s => s.UniqueId == data.pollutant_id);
            if (sensor == null)
            {
                _logger.LogInformation("ShmuService: Sensor {SensorId} not found", data.pollutant_id);
                continue;
            }

            var isReadingInDb = await _bpContext.Reading.AnyAsync(r =>
                r.SensorId == sensor.Id && r.DateTime == DateTimeOffset.FromUnixTimeSeconds(data.dt));

            if (isReadingInDb)
                continue;

            var reading = new Reading
            {
                Sensor = sensor,
                DateTime = DateTimeOffset.FromUnixTimeSeconds(data.dt).DateTime,
                Value = decimal.Parse(data.value)
            };

            await _bpContext.Reading.AddAsync(reading);
            await _bpContext.SaveChangesAsync();
        }
    }

    public async Task<List<Station>> GetStations()
    {
        var response =
            await Requests.Get<List<ShmuAirResponse>>(
                "https://www.shmu.sk/api/v1/airquality/getdata?history=0&station_meta=1");
        if (response == null || !response.Any())
        {
            _logger.LogError("ShmuService: Failed to get data");
            throw new Exception("Failed to get data");
        }

        return response.Select(r => r.station).ToList();
    }
}