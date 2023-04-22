namespace BP.Data.Models.Sensor;

public class SensorData
{
    public string esp8266id { get; set; }
    public string software_version { get; set; }
    public List<SensorDataValue> sensordatavalues { get; set; }
}

public class SensorDataValue
{
    public string value_type { get; set; }
    public string value { get; set; }
}