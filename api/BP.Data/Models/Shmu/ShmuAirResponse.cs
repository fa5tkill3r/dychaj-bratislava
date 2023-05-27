// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

using Newtonsoft.Json;

#pragma warning disable CS8618

namespace BP.Data.Models.Shmu;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class ShmuAirResponse
{
    public Station station { get; set; }
    public List<Data> data { get; set; }
    public List<ExtraData> extra_data { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Pollutant
{
    public string pollutant_id { get; set; }
    public string pollutant_desc { get; set; }
    public int order { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Station
{
    public int station_id { get; set; }
    public string station_name { get; set; }
    public int type { get; set; }
    public int has_iko { get; set; }
    public double? gps_lat { get; set; }
    public double? gps_lon { get; set; }
    public List<Pollutant> pollutants { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Data
{
    public long dt { get; set; }
    public string? value { get; set; }
    public string pollutant_id { get; set; }
    public int limit_level { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class ExtraData
{
    public long dt { get; set; }
    public int value_avg { get; set; }
    public int value_max { get; set; }
    public int value { get; set; }
    public string pollutant_id { get; set; }
}