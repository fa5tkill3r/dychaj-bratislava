namespace BP.Data.Dto.Response.Stats;

public class PM25StatsResponse
{
    public List<LocationDto> Locations { get; set; }
    public DateTime TimeOver { get; set; }
    public float Value { get; set; }
    public float HourValueAvg { get; set; }
    public float DayValue { get; set; }
}