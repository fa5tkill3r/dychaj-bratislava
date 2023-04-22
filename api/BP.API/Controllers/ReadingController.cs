using BP.API.Services;
using BP.Data.Models.Sensor;
using Microsoft.AspNetCore.Mvc;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ReadingController : ControllerBase
{
    private readonly ReadingService _readingService;

    public ReadingController(ReadingService readingService)
    {
        _readingService = readingService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddReading([FromBody] SensorData sensorData)
    {
        await _readingService.AddReading(sensorData);
        return Ok();
    }

    
    [HttpGet]
    public async Task<IActionResult> GetReadings()
    {
        return Ok("Hello World!");
    }
    

    
}