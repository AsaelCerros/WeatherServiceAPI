using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherService.Controllers;
using WeatherService.Interfaces;
using WeatherService.Models;

namespace WeatherService.Tests
{
    public class WeatherControllerTests
    {
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly Mock<IGeocodingService> _mockGeocodingService;
        private readonly Mock<ILogger<WeatherController>> _mockLogger;
        private readonly WeatherController _controller;

        public WeatherControllerTests()
        {
            _mockWeatherService = new Mock<IWeatherService>();
            _mockGeocodingService = new Mock<IGeocodingService>();
            _mockLogger = new Mock<ILogger<WeatherController>>();
            _controller = new WeatherController(_mockWeatherService.Object, _mockGeocodingService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetWeatherByCoordinates_ShouldReturnOkResult_WhenCoordinatesAreValid()
        {
            var latitude = 50.0;
            var longitude = 50.0;
            var expectedWeatherData = new WeatherData
            {
                Temperature = 20.5,
                WindDirection = 180.0,
                WindSpeed = 5.5,
                Sunrise = DateTime.UtcNow
            };

            _mockWeatherService.Setup(x => x.GetWeatherDataAsync(latitude, longitude))
                .ReturnsAsync(expectedWeatherData);


            var result = await _controller.GetWeatherByCoordinates(latitude, longitude);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedWeatherData = Assert.IsType<WeatherData>(okResult.Value);
            Assert.Equal(expectedWeatherData.Temperature, returnedWeatherData.Temperature);
            Assert.Equal(expectedWeatherData.WindDirection, returnedWeatherData.WindDirection);
            Assert.Equal(expectedWeatherData.WindSpeed, returnedWeatherData.WindSpeed);
            Assert.Equal(expectedWeatherData.Sunrise, returnedWeatherData.Sunrise);
        }

        [Fact]
        public async Task GetWeatherByCoordinates_ShouldReturnBadRequest_WhenLatitudeIsInvalid()
        {
            var latitude = 100.0;
            var longitude = 50.0;


            var result = await _controller.GetWeatherByCoordinates(latitude, longitude);


            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetWeatherByCoordinates_ShouldReturnBadRequest_WhenLongitudeIsInvalid()
        {
            var latitude = 50.0;
            var longitude = 200.0;


            var result = await _controller.GetWeatherByCoordinates(latitude, longitude);


            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetWeatherByCoordinates_ShouldReturnStatusCode500_WhenServiceThrowsException()
        {
            var latitude = 50.0;
            var longitude = 50.0;

            _mockWeatherService.Setup(x => x.GetWeatherDataAsync(latitude, longitude))
                .ThrowsAsync(new Exception("Test exception"));


            var result = await _controller.GetWeatherByCoordinates(latitude, longitude);


            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetWeatherByCity_ShouldReturnOkResult_WhenCityIsValid()
        {
            var city = "London";
            var latitude = 51.5074;
            var longitude = -0.1278;
            var expectedWeatherData = new WeatherData
            {
                Temperature = 20.5,
                WindDirection = 180.0,
                WindSpeed = 5.5,
                Sunrise = DateTime.UtcNow
            };

            _mockGeocodingService.Setup(x => x.GetCoordinatesForCityAsync(city))
                .ReturnsAsync((latitude, longitude));
            _mockWeatherService.Setup(x => x.GetWeatherDataAsync(latitude, longitude))
                .ReturnsAsync(expectedWeatherData);


            var result = await _controller.GetWeatherByCity(city);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedWeatherData = Assert.IsType<WeatherData>(okResult.Value);
            Assert.Equal(expectedWeatherData.Temperature, returnedWeatherData.Temperature);
            Assert.Equal(expectedWeatherData.WindDirection, returnedWeatherData.WindDirection);
            Assert.Equal(expectedWeatherData.WindSpeed, returnedWeatherData.WindSpeed);
            Assert.Equal(expectedWeatherData.Sunrise, returnedWeatherData.Sunrise);
        }

        [Fact]
        public async Task GetWeatherByCity_ShouldReturnNotFound_WhenCityCoordinatesAreNotFound()
        {
            var city = "NonexistentCity";

            _mockGeocodingService.Setup(x => x.GetCoordinatesForCityAsync(city))
                .ReturnsAsync(() => null);


            var result = await _controller.GetWeatherByCity(city);


            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetWeatherByCity_ShouldReturnBadRequest_WhenCityNameIsEmpty()
        {
            var city = "";


            var result = await _controller.GetWeatherByCity(city);


            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetWeatherByCity_ShouldReturnStatusCode500_WhenGeocodingServiceThrowsException()
        {
            var city = "London";

            _mockGeocodingService.Setup(x => x.GetCoordinatesForCityAsync(city))
                .ThrowsAsync(new Exception("Test exception"));


            var result = await _controller.GetWeatherByCity(city);


            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetWeatherByCity_ShouldReturnStatusCode500_WhenWeatherServiceThrowsException()
        {
            var city = "London";
            var latitude = 51.5074;
            var longitude = -0.1278;

            _mockGeocodingService.Setup(x => x.GetCoordinatesForCityAsync(city))
                .ReturnsAsync((latitude, longitude));
            _mockWeatherService.Setup(x => x.GetWeatherDataAsync(latitude, longitude))
                .ThrowsAsync(new Exception("Test exception"));


            var result = await _controller.GetWeatherByCity(city);


            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}