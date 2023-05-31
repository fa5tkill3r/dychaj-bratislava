using Microsoft.EntityFrameworkCore;

namespace BP.Data.DbModels;

[Index(nameof(DateTime))]
public class Reading
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public Sensor Sensor { get; set; } = null!;
    public decimal Value { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}