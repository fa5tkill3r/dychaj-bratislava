namespace BP.Data.Dto.Response;

public class Pm25ExceedResponse
{
    public ModuleDto Module { get; set; }
    public int Exceed { get; set; }
}