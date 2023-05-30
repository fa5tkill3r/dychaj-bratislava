namespace BP.Data.Dto.Response.Stats;

public class BasicStatModule
{
    public ModuleDto Module { get; set; }
    public decimal? Current { get; set; }
    public decimal? Max { get; set; }
    public decimal? Min { get; set; }
}