namespace BP.Data.DbModels;

public class Sensor
{
    public int Id { get; set; }
    public string UniqueId { get; set; }
    public int ModuleId { get; set; }
    public Module Module { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Unit { get; set; }
    public List<Reading> Readings { get; set; }
}