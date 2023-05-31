namespace BP.Data.Dto.Request;

public class Pm25CompareRequest
{
    public List<int> Sensors { get; set; } = null!;
    public List<DayOfWeek> WeekDays { get; set; } = null!;
    public List<int> Hours { get; set; } = null!;
    public int Weeks { get; set; }
}