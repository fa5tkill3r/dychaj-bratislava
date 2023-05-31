namespace BP.Data.Dto.Response.Stats;

public class PmStatsResponse
{
    public List<PmStatsSensor> Sensors { get; set; } = new();
}

public class PmStatsSensor
{
    public SensorDto Sensor { get; set; } = null!;
    public decimal? YearValueAvg { get; set; }
    public decimal? DayValueAvg { get; set; }
    public decimal? Current { get; set; }
    public decimal? DaysAboveThreshold { get; set; }
}