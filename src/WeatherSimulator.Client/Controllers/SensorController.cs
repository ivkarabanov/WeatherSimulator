using Microsoft.AspNetCore.Mvc;
using RateLimiterCore;
using WeatherSimulator.Client.Exceptions;
using WeatherSimulator.Client.LimiterInfrastructure;
using WeatherSimulator.Client.Services;
using WeatherSimulator.Core.Models;

namespace WeatherSimulator.Client.Controllers;

[Route("sensor")]
public class MeasureCurrentController : ControllerBase
{
    private readonly ISensorDataService _sensorDataService;

    public MeasureCurrentController(ISensorDataService sensorDataService)
    {
        _sensorDataService = sensorDataService;
    }
    
    [HttpGet("{sensorNumber}/current")]
    [StatusCodeException]
    [ServiceFilter(typeof(RateLimiterAttribute))]
    [ProducesResponseType(typeof(SensorMeasure),StatusCodes.Status200OK )]
    [ProducesResponseType(StatusCodes.Status400BadRequest )]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests )]
    [ProducesResponseType(StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> GetMeasure(int sensorNumber)
    {
        var measure = await _sensorDataService.GetCurrentAsync(sensorNumber);
        return Ok(measure);
    }
    [HttpGet("{sensorNumber}/history")]
    [StatusCodeException]
    [ServiceFilter(typeof(RateLimiterAttribute))]
    [ProducesResponseType(typeof(IEnumerable<SensorMeasure>),StatusCodes.Status200OK )]
    [ProducesResponseType(StatusCodes.Status400BadRequest )]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests )]
    [ProducesResponseType(StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> History(int sensorNumber)
    {
        var sensorMeasureData = await _sensorDataService.GetHistoryAsync(sensorNumber);
        return Ok(sensorMeasureData);
    }
}