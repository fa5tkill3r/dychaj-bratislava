using Microsoft.AspNetCore.Mvc;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetReadings()
    {
        return Task.FromResult<IActionResult>(Ok("Ok"));
    }
}