using BP.Data.DbHelpers;
using ValueType = System.ValueType;

namespace BP.Data.Dto.Response;

public class SensorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public LocationDto Location { get; set; }
    public string? LocationName { get; set; }
    public string? Description { get; set; }
    public Source Source { get; set; }

    public ValueType Type { get; set; }
    // public List<MeasurementDto> Measurements { get; set; } = null!;
    // public List<ModuleDto> Modules { get; set; } = null!;
}