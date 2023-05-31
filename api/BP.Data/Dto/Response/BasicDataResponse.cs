namespace BP.Data.Dto.Response;

public class BasicDataResponse
{
    public List<SensorWithReadingsDto> Sensors { get; set; } = new();
}