using ValueType = BP.Data.Models.ValueType;

namespace BP.API.Utility;

public class Helpers
{
    public static ValueType GetTypeFromString(string valueType) => valueType.ToLower().Split('_').Last() switch
    {
        "temperature" => ValueType.TEMP,
        "humidity" => ValueType.HUMIDITY,
        "pressure" => ValueType.PRESSURE,
        "p1" => ValueType.PM10,
        "p2" => ValueType.PM25,
        _ => ValueType.UNKNOWN
    };
}