// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace BP.Data.Models.Shmu;

public abstract class ShmuResponse
{
    public Station station { get; set; }
    public List<object> data { get; set; }
    public List<object> extra_data { get; set; }
}

public abstract class Pollutant
{
    public string pollutant_id { get; set; }
    public string pollutant_desc { get; set; }
    public int order { get; set; }
}

public abstract class Station
{
    public int station_id { get; set; }
    public string station_name { get; set; }
    public int type { get; set; }
    public int has_iko { get; set; }
    public double? gps_lat { get; set; }
    public double? gps_lon { get; set; }
    public List<Pollutant> pollutants { get; set; }
}
