namespace BP.Data.Models.SensorCommunity;

public class SensorCommunity
{
    public Location location { get; set; }
    public object id { get; set; }
    public List<SensorDataValue> sensordatavalues { get; set; }
    public string timestamp { get; set; }
    public Sensor sensor { get; set; }
    public object sampling_rate { get; set; }
}