using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WeatherService.Services;
using System.Net;
using System.Text.Json;

namespace WeatherService.Tests
{
    public class GeocodingServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<GeocodingService>> _mockLogger;

        public GeocodingServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<GeocodingService>>();
        }

        [Fact]
        public async Task GetCoordinatesForCityAsync_ShouldReturnCoordinates_WhenCityIsFound()
        {
            var city = "London";
            var expectedLatitude = 51.5074;
            var expectedLongitude = -0.1278;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        results = new[]
                        {
                            new
                            {
                                name = "London",
                                latitude = expectedLatitude,
                                longitude = expectedLongitude
                            }
                        }
                    }))
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var geocodingService = new GeocodingService(_mockHttpClientFactory.Object, _mockLogger.Object);


            var result = await geocodingService.GetCoordinatesForCityAsync(city);


            Assert.NotNull(result);
            Assert.Equal(expectedLatitude, result.Value.Latitude);
            Assert.Equal(expectedLongitude, result.Value.Longitude);
        }

        [Fact]
        public async Task GetCoordinatesForCityAsync_ShouldReturnNull_WhenCityIsNotFound()
        {
            var city = "NonexistentCity";

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        results = Array.Empty<object>()
                    }))
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var geocodingService = new GeocodingService(_mockHttpClientFactory.Object, _mockLogger.Object);


            var result = await geocodingService.GetCoordinatesForCityAsync(city);


            Assert.Null(result);
        }
    }
}