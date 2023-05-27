namespace BP.Data.Dto.Response;

public class ModuleWithReadingsDto : ModuleDto
{
    public List<ReadingDto> Readings { get; set; }
}