using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Utility;

public class Helpers
{
    public static ValueType GetTypeFromString(string valueType)
    {
        return valueType.ToLower().Split('_').Last() switch
        {
            "temperature" => ValueType.Temp,
            "humidity" => ValueType.Humidity,
            "pressure" => ValueType.Pressure,
            "p1" => ValueType.Pm10,
            "p2" => ValueType.Pm25,
            "pm10" => ValueType.Pm10,
            "pm2.5" => ValueType.Pm25,
            _ => ValueType.Unknown
        };
    }
}