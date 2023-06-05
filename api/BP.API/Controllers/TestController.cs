using Microsoft.AspNetCore.Mvc;

namespace BP.API.Controllers;

[ApiController]
[Route("")]
public class TestController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetVersion()
    {
        var version = GetType().Assembly.GetName().Version;
        return Task.FromResult<IActionResult>(Ok(version));
    }
}