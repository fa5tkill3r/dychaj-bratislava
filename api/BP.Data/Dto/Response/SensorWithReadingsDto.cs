namespace BP.Data.Dto.Response;

public class SensorWithReadingsDto : SensorDto
{
    public List<ReadingDto> Readings { get; set; } = null!;
}