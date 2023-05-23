namespace BP.Data.Dto.Response.Stats;

public class PM25StatsResponse
{
    public List<ModuleDto> Modules { get; set; }
    public decimal YearValueAvg { get; set; }
    public decimal DayValueAvg { get; set; }
    public decimal Current { get; set; }
}