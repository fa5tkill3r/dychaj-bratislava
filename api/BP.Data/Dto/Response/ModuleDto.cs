namespace BP.Data.Dto.Response;

public class ModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public LocationDto? Location { get; set; }
}