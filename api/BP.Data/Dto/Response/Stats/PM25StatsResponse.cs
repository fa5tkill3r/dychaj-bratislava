namespace BP.Data.Dto.Response.Stats;

public class PM25StatsResponse
{
    public List<PM25StatsResponseModule> Modules { get; set; } = new();
    public List<ModuleDto> AvailableModules { get; set; } = new();
}

public class PM25StatsResponseModule
{
    public ModuleDto Module { get; set; }
    public decimal? YearValueAvg { get; set; }
    public decimal? DayValueAvg { get; set; }
    public decimal? Current { get; set; }
    public decimal? DaysAboveThreshold { get; set; }
}