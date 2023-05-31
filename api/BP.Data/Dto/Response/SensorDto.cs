using BP.Data.DbHelpers;
using ValueType = System.ValueType;

namespace BP.Data.Dto.Response;

public class SensorDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public LocationDto Location { get; set; }
    public ModuleDto Module { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; }
}