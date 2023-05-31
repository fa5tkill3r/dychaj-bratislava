namespace BP.Data.Dto.Response.Stats;

public class BasicStatSensor
{
    public SensorDto Sensor { get; set; } = null!;
    public decimal? Current { get; set; }
    public decimal? Max { get; set; }
    public decimal? Min { get; set; }
}