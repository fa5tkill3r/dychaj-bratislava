namespace BP.Data.DbModels;

public class Module
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public List<Sensor> Sensors { get; set; }
}