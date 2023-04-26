using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models.Sensor;
using Microsoft.EntityFrameworkCore;

namespace BP.API.Services;

public class ValueService
{
    private readonly Context _context;

    private string[] ignoreValues = {
        "samples",
        "min_micro",
        "max_micro",
        "signal",
    };

    public ValueService(Context context)
    {
        _context = context;
    }

    public async Task AddEspValue(SensorData sensorData)
    {
        var module = await _context.Module
            .Include(e => e.Sensors)
            .Where(m => m.Source == Source.Esp)
            .FirstOrDefaultAsync(e => e.UniqueId == sensorData.esp8266id);
        if (module == null)
        {
            module = new Module()
            {
                Name = sensorData.esp8266id,
                UniqueId = sensorData.esp8266id,
                LocationId = null,
                Source = Source.Esp,
            };
            await _context.Module.AddAsync(module);
            await _context.SaveChangesAsync();
        }

        await _context.Entry(module).Collection(m => m.Sensors).LoadAsync();

        foreach (var sensorDataVal in sensorData.sensordatavalues)
        {
            if (ignoreValues.Contains(sensorDataVal.value_type))
                continue;

            var sensor = module.Sensors.FirstOrDefault(s => s.UniqueId == sensorDataVal.value_type);
            if (sensor == null)
            {
                sensor = new Sensor()
                {
                    Name = sensorDataVal.value_type,
                    Unit = sensorDataVal.value_type,
                    UniqueId = sensorDataVal.value_type,
                    ModuleId = module.Id,
                };
                await _context.Sensor.AddAsync(sensor);
                await _context.SaveChangesAsync();
            }

            var reading = new Reading()
            {
                SensorId = sensor.Id,
                Value = decimal.Parse(sensorDataVal.value),
            };
            await _context.Reading.AddAsync(reading);
        }

        await _context.SaveChangesAsync();
    }
    
    
    
    
}