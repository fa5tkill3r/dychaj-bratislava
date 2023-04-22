namespace BP.Data.DbModels;

public class Reading
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public Sensor Sensor { get; set; }
    public decimal Value { get; set; }
}