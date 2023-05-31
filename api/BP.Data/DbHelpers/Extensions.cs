using BP.Data.DbModels;

namespace BP.Data.DbHelpers;

public static class Extensions
{
    public static string GetName(this Sensor sensor)
    {
        var name = sensor.Module.Name;
        if (!string.IsNullOrEmpty(sensor.Name))
        {
            name += $" - {sensor.Name}";
        }
        
        return name;
    }
}