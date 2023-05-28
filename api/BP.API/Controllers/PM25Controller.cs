using BP.API.Services;
using BP.Data.Dto.Request;
using Microsoft.AspNetCore.Mvc;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PM25Controller : ControllerBase
{
    private readonly Pm25Service _pm25Service;

    public PM25Controller(Pm25Service pm25Service)
    {
        _pm25Service = pm25Service;
    }

    [HttpGet]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await _pm25Service.GetLocations();
        return Ok(locations);
    }

    [HttpPost]
    [Route("stats")]
    public async Task<IActionResult> GetStats([FromBody] Pm25StatsRequest? request)
    {
        return Ok(await _pm25Service.GetStats(request));
    }
    
    [HttpPost]
    [Route("weekly")]
    public async Task<IActionResult> GetWeeklyComparison([FromBody] Pm25WeeklyComparisonRequest? request)
    {
        return Ok(await _pm25Service.GetWeeklyComparison(request));
    }
    
    [HttpGet]
    [Route("map")]
    public async Task<IActionResult> GetMap()
    {
        return Ok(await _pm25Service.GetMap());
    }
}