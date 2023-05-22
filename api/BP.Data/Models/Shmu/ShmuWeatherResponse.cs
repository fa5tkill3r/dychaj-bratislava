// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

#pragma warning disable CS8618

namespace BP.Data.Models.Shmu;

public class ShmuWeatherResponse
{
    public string type { get; set; }
    public Crs crs { get; set; }
    public List<Feature> features { get; set; }
}

public class Crs
{
    public string type { get; set; }
    public Properties properties { get; set; }
}

public class Feature
{
    public string type { get; set; }
    public int id { get; set; }
    public Properties properties { get; set; }
    public Geometry geometry { get; set; }
}

public class Geometry
{
    public string type { get; set; }
    public List<double> coordinates { get; set; }
}

public class Properties
{
    public string prop_name { get; set; }
    public PropWeather prop_weather { get; set; }
}

public class PropWeather
{
    public string station_id { get; set; }
    public long dt { get; set; }
    public double? ttt { get; set; }
    public string? v_smer { get; set; }
    public int? v_rychlost { get; set; }
    public object? zra { get; set; }
    public string? zra_1h { get; set; }
    public string? tlak { get; set; }
    public string? trb { get; set; }
    public string? rh { get; set; }
    public object? fx { get; set; }
    public object? fm { get; set; }
    public string? synop_n { get; set; }
    public string? synop_ww { get; set; }
    public int? icon_id { get; set; }
}