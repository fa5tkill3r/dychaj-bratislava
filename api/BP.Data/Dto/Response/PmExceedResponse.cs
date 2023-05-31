namespace BP.Data.Dto.Response;

public class PmExceedResponse
{
    public SensorDto Sensor { get; set; } = null!;
    public int Exceed { get; set; }
}