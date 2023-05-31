using BP.API.Services;
using BP.Data.Dto.Request;
using Microsoft.AspNetCore.Mvc;
using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class Pm25Controller : ControllerBase
{
    private readonly Pm25Service _pm25Service;
    private readonly BasicService _basicService;

    public Pm25Controller(Pm25Service pm25Service, BasicService basicService)
    {
        _pm25Service = pm25Service;
        _basicService = basicService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await _basicService.GetLocations(ValueType.Pm25);
        return Ok(locations);
    }

    [HttpPost]
    [Route("stats")]
    public async Task<IActionResult> GetStats([FromBody] StatsRequest? request)
    {
        return Ok(await _pm25Service.GetStats(request));
    }
    
    [HttpPost]
    public async Task<IActionResult> GetData([FromBody] GetDataRequest? request)
    {
        return Ok(await _basicService.GetData(ValueType.Pm25, request));
    }
    
    [HttpGet]
    [Route("map")]
    public async Task<IActionResult> GetMap()
    {
        return Ok(await _basicService.GetMap(ValueType.Pm25));
    }
    
    [HttpGet]
    [Route("exceed")]
    public async Task<IActionResult> GetYearlyExceed()
    {
        return Ok(await _pm25Service.GetYearlyExceed());
    }
    
    [HttpPost]
    [Route("compare")]
    public async Task<IActionResult> GetCompare([FromBody] Pm25CompareRequest request)
    {
        return Ok(await _pm25Service.GetCompare(request));
    }
}