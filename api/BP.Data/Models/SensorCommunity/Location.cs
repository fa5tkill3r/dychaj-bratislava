// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CS8618
using Newtonsoft.Json;

namespace BP.Data.Models.SensorCommunity;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Location
{
    public int id { get; set; }
    public string country { get; set; }
    public double altitude { get; set; }
    public int indoor { get; set; }
    public double longitude { get; set; }
    public double latitude { get; set; }
    public int exact_location { get; set; }
}