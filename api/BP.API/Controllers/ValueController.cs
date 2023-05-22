using BP.API.Services;
using BP.Data.Models.Sensor;
using Microsoft.AspNetCore.Mvc;

namespace BP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ValueController : ControllerBase
{
    private readonly ValueService _valueService;

    public ValueController(ValueService valueService)
    {
        _valueService = valueService;
    }

    [HttpPost]
    public async Task<IActionResult> AddValue([FromBody] SensorData sensorData)
    {
        await _valueService.AddEspValue(sensorData);
        return Ok();
    }


    [HttpGet]
    public async Task<IActionResult> GetReadings()
    {
        return Ok("Hello World!");
    }
}