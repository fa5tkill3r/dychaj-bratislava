namespace BP.Data.Models.SensorCommunity;

public class Location
{
    public int id { get; set; }
    public string country { get; set; }
    public string altitude { get; set; }
    public int indoor { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
    public int exact_location { get; set; }
}