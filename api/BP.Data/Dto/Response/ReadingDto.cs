namespace BP.Data.Dto.Response;

public class ReadingDto
{
    public DateTimeOffset DateTime { get; set; }
    public decimal? Value { get; set; }
}