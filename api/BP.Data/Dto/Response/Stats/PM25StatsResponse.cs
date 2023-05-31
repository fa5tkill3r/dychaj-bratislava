namespace BP.Data.Dto.Response.Stats;

public class Pm25StatsResponse
{
    public List<Pm25StatsSensor> Sensors { get; set; } = new();
}

public class Pm25StatsSensor
{
    public SensorDto Sensor { get; set; } = null!;
    public decimal? YearValueAvg { get; set; }
    public decimal? DayValueAvg { get; set; }
    public decimal? Current { get; set; }
    public decimal? DaysAboveThreshold { get; set; }
}