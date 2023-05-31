namespace BP.Data.Models;

public class GetSensorsDto
{
    public string UniqueId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ValueType Type { get; set; } = null!;
}