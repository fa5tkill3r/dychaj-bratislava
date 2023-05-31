// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CS8618
namespace BP.Data.Models.SensorCommunity;

public class Sensor
{
    public int id { get; set; }
    public string pin { get; set; }
    public SensorType sensor_type { get; set; }
}