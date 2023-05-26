using BP.API.Services;
using BP.Data.Dto.Request;
using Microsoft.AspNetCore.Mvc;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PM25Controller : ControllerBase
{
    private readonly PM25Service _pm25Service;

    public PM25Controller(PM25Service pm25Service)
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
    public async Task<IActionResult> GetStats([FromBody] PM25StatsRequest request)
    {
        return Ok(await _pm25Service.GetStats(request));
    }
}