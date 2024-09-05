using Microsoft.AspNetCore.Mvc;
using WeatherService.Interfaces;
using WeatherService.Models;
using WeatherService.Services;

namespace WeatherService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IGeocodingService _geocodingService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IWeatherService weatherService, IGeocodingService geocodingService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _geocodingService = geocodingService;
            _logger = logger;
        }

        /// <summary>
        /// Get weather data based on latitude and longitude
        /// </summary>
        /// <param name="latitude">Latitude of the location</param>
        /// <param name="longitude">Longitude of the location</param>
        /// <returns>Weather data including temperature, wind direction, wind speed, and sunrise time</returns>
        [HttpGet("coordinates")]
        public async Task<ActionResult<WeatherData>> GetWeatherByCoordinates(
            [FromQuery] double latitude,
            [FromQuery] double longitude)
        {
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                _logger.LogWarning("Invalid coordinates provided: {Latitude}, {Longitude}", latitude, longitude);
                return BadRequest("Invalid latitude or longitude values");
            }

            try
            {
                var weatherData = await _weatherService.GetWeatherDataAsync(latitude, longitude);
                return Ok(weatherData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                return StatusCode(500, "An error occurred while fetching weather data. Please try again later.");
            }
        }

        /// <summary>
        /// Get weather data based on city name
        /// </summary>
        /// <param name="city">Name of the city</param>
        /// <returns>Weather data including temperature, wind direction, wind speed, and sunrise time</returns>
        [HttpGet("city")]
        public async Task<ActionResult<WeatherData>> GetWeatherByCity([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                _logger.LogWarning("Empty city name provided");
                return BadRequest("City name cannot be empty");
            }

            try
            {
                var coordinates = await _geocodingService.GetCoordinatesForCityAsync(city);
                if (coordinates == null)
                {
                    return NotFound($"Could not find coordinates for the city: {city}");
                }

                var (latitude, longitude) = coordinates.Value;
                var weatherData = await _weatherService.GetWeatherDataAsync(latitude, longitude);
                return Ok(weatherData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for city: {City}", city);
                return StatusCode(500, "An error occurred while fetching weather data. Please try again later.");
            }
        }
    }
}