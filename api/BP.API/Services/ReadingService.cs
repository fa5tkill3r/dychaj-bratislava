using BP.Data;
using BP.Data.DbModels;
using BP.Data.DbModels.Modules;
using BP.Data.Models.Sensor;
using Microsoft.EntityFrameworkCore;

namespace BP.API.Services;


public class ReadingService
{
    private readonly Context _context;

    public ReadingService(Context context)
    {
        _context = context;
    }
    
    public async Task AddReading(SensorData sensorData)
    {
        var module = await _context.Esp
            .Include(e => e.Sensors)
            .FirstOrDefaultAsync(e => e.EspId == sensorData.esp8266id);
        if (module == null)
        {
            module = new Esp()
            {
                Name = sensorData.esp8266id,
                UniqueId = sensorData.esp8266id,
                LocationId = null,
            };
            await _context.Module.AddAsync(module);
        }
        
        // module load sensors
        await _context.Entry(module).Collection(m => m.Sensors).LoadAsync();

        foreach (var sensorDataVal in sensorData.sensordatavalues)
        {
            var sensor = module.Sensors.FirstOrDefault(s => s.Unit == sensorDataVal.value_type);
            if (sensor == null)
            {
                sensor = new Sensor()
                {
                    Name = sensorDataVal.value_type,
                    Description = sensorDataVal.value_type,
                    Unit = sensorDataVal.value_type,
                    ModuleId = module.Id,
                };
                await _context.Sensor.AddAsync(sensor);
            }
            
            sensorDataVal.value = sensorDataVal.value.Replace('.', ',');
            
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