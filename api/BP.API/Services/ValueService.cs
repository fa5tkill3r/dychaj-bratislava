using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models.Sensor;
using Microsoft.EntityFrameworkCore;
using static BP.API.Utility.Helpers;

namespace BP.API.Services;

public class ValueService
{
    private readonly BpContext _bpContext;

    private readonly string[] ignoreValues =
    {
        "samples",
        "min_micro",
        "max_micro",
        "signal"
    };

    public ValueService(BpContext bpContext)
    {
        _bpContext = bpContext;
    }

    public async Task AddEspValue(SensorData sensorData)
    {
        var module = await _bpContext.Module
            .Include(e => e.Sensors)
            .Where(m => m.Source == Source.Esp)
            .FirstOrDefaultAsync(e => e.UniqueId == sensorData.esp8266id);
        if (module == null)
        {
            module = new Module
            {
                Name = sensorData.esp8266id,
                UniqueId = sensorData.esp8266id,
                LocationId = null,
                Source = Source.Esp
            };
            await _bpContext.Module.AddAsync(module);
            await _bpContext.SaveChangesAsync();
        }

        await _bpContext.Entry(module).Collection(m => m.Sensors).LoadAsync();

        foreach (var sensorDataVal in sensorData.sensordatavalues)
        {
            if (ignoreValues.Contains(sensorDataVal.value_type))
                continue;

            var sensor = module.Sensors.FirstOrDefault(s => s.UniqueId == sensorDataVal.value_type);
            if (sensor == null)
            {
                sensor = new Sensor
                {
                    Name = sensorDataVal.value_type,
                    Type = GetTypeFromString(sensorDataVal.value_type),
                    UniqueId = sensorDataVal.value_type,
                    ModuleId = module.Id
                };
                await _bpContext.Sensor.AddAsync(sensor);
                await _bpContext.SaveChangesAsync();
            }

            var reading = new Reading
            {
                SensorId = sensor.Id,
                Value = decimal.Parse(sensorDataVal.value)
            };
            await _bpContext.Reading.AddAsync(reading);
        }

        await _bpContext.SaveChangesAsync();
    }
}