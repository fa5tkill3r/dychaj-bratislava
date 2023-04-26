using BP.API.Utility;
using BP.Data;
using BP.Data.DbHelpers;
using BP.Data.DbModels;
using BP.Data.Models.SensorCommunity;
using Microsoft.EntityFrameworkCore;

namespace BP.API.Services;

public class SensorCommunityService
{
    private readonly Context _context;

    public SensorCommunityService(Context context)
    {
        _context = context;
    }
    
    public async Task GetData()
    {
        var modules = await _context.Module
            .Where(m => m.Source == Source.SensorCommunity)
            .Include(e => e.Sensors)
            .ToListAsync();

        foreach (var module in modules)
        {
            foreach (var sensor in module.Sensors)
            {
                var x = await Requests.Get<List<SensorCommunity>>($"https://data.sensor.community/airrohr/v1/sensor/{sensor.UniqueId}/");
            }
        }
    }
    
    
}