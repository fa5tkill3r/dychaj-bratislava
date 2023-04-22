using BP.Data;
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
    
    public Task AddReading(SensorData sensorData)
    {
        throw new NotImplementedException();
    }
}