using Microsoft.AspNetCore.Mvc;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetReadings()
    {
        return Ok("Ok");
    }
}