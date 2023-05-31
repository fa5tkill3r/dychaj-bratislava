// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable IdentifierTypo
#pragma warning disable CS8618
namespace BP.Data.Models.SensorCommunity;

public class SensorCommunity
{
    public Location location { get; set; }
    public long id { get; set; }
    public List<SensorDataValue> sensordatavalues { get; set; }
    public DateTime timestamp { get; set; }
    public Sensor sensor { get; set; }
    public object sampling_rate { get; set; }
}