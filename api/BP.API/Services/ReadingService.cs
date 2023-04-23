using BP.Data;
using BP.Data.DbModels;
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
        var module = await _context.Module.FirstOrDefaultAsync(m => m.UniqueId == sensorData.esp8266id);
        if (module == null)
        {
            module = new Module
            {
                UniqueId = sensorData.esp8266id,
                Name = sensorData.esp8266id,
                LocationId = null,
            };
            await _context.Module.AddAsync(module);
        }
        
        // module load sensors
        // await _context.Entry(module).Collection(m => m.Sensors).LoadAsync();

        foreach (var sensorDataVal in sensorData.sensordatavalues)
        {
            var moduleSensor = module.Sensors.FirstOrDefault(s => s.UniqueId == sensorDataVal.value_type);
            if (moduleSensor == null)
            {
                moduleSensor = new Sensor
                {
                    UniqueId = sensorDataVal.value_type,
                    Name = sensorDataVal.value_type,
                    Unit = sensorDataVal.value_type,
                    ModuleId = module.Id,
                };
                await _context.Sensor.AddAsync(moduleSensor);
            }
            
        }
        
        throw new NotImplementedException();
    }
}