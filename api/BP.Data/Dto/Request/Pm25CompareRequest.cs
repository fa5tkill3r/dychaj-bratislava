namespace BP.Data.Dto.Request;

public class Pm25CompareRequest
{
    public List<int> Modules { get; set; }
    public List<DayOfWeek> WeekDays { get; set; }
    public List<int> Hours { get; set; }
    public int Weeks { get; set; }
}