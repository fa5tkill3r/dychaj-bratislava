namespace BP.Data.Dto.Request;

public class GetDataRequest
{
    public int[]? Sensors { get; set; }
    public Interval Interval { get; set; } = Interval.TenMinutes;
    public DateTimeOffset From { get; set; } = DateTimeOffset.UtcNow.AddDays(-1);
    public DateTimeOffset To { get; set; } = DateTimeOffset.UtcNow;
}