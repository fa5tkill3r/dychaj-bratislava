namespace BP.API.Utility;

public class Helpers
{
    public static string GetUnitFromType(string valueType) => valueType switch
    {
        "temperature" => "°C",
        "humidity" => "%",
        "pressure" => "hPa",
        _ => valueType
    };
}