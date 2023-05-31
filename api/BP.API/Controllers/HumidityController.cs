using BP.API.Services;
using BP.Data.Dto.Request;
using Microsoft.AspNetCore.Mvc;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HumidityController : ControllerBase
{
    private readonly BasicService _basicService;
    private const ValueType ValueType = Data.DbHelpers.ValueType.Humidity;

    public HumidityController(BasicService basicService)
    {
        _basicService = basicService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await _basicService.GetLocations(ValueType);
        return Ok(locations);
    }
    
    [HttpPost]
    [Route("stats")]
    public async Task<IActionResult> GetStats([FromBody] StatsRequest? request)
    {
        return Ok(await _basicService.GetStats(ValueType, request));
    }
    
    [HttpPost]
    public async Task<IActionResult> GetData([FromBody] GetDataRequest? request)
    {
        return Ok(await _basicService.GetData(ValueType, request));
    }
    
    [HttpGet]
    [Route("map")]
    public async Task<IActionResult> GetMap()
    {
        return Ok(await _basicService.GetMap(ValueType));
    }
}