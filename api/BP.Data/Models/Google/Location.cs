namespace BP.Data.Models.Google;

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = null!;
    public string StreetName { get; set; } = null!;
}