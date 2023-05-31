// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CS8618
namespace BP.Data.Models.SensorCommunity;

public class SensorDataValue
{
    public decimal value { get; set; }
    public long id { get; set; }
    public string value_type { get; set; }
}