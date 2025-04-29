using Microsoft.AspNetCore.Mvc;
using WeatherService.Services;

namespace WeatherService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpPost("location")]
        public async Task<IActionResult> SetUserLocation([FromQuery] string userId, [FromQuery] double latitude, [FromQuery] double longitude)
        {
            await _weatherService.SaveOrUpdateUserLocationAsync(userId, latitude, longitude);
            return Ok("Location saved/updated.");
        }

        [HttpGet("forecast")]
        public async Task<ActionResult<string>> GetWeather([FromQuery] string userId)
        {
            var forecastJson = await _weatherService.GetWeatherAsync(userId);
            return Ok(forecastJson);
        }
    }
}
