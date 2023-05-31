namespace BP.Data.Dto.Response;

public class Pm25WeeklyComparisonResponse
{
    public List<SensorWithReadingsDto> Sensors { get; set; } = new();
    public List<ModuleDto> AvailableModules { get; set; } = new();
}