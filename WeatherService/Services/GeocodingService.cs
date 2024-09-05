using System.Text.Json;
using WeatherService.Interfaces;

namespace WeatherService.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeocodingService> _logger;
        private const string OpenMeteoGeocodingApiUrl = "https://geocoding-api.open-meteo.com/v1/search";

        // Set up our HTTP client and logger
        public GeocodingService(IHttpClientFactory httpClientFactory, ILogger<GeocodingService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<(double Latitude, double Longitude)?> GetCoordinatesForCityAsync(string city)
        {
            try
            {
                // Build the URL for the geocoding API
                var url = $"{OpenMeteoGeocodingApiUrl}?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
                
                // Parse the response
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    // Grab the first result
                    var result = results[0];
                    var latitude = result.GetProperty("latitude").GetDouble();
                    var longitude = result.GetProperty("longitude").GetDouble();
                    
                    _logger.LogInformation("Got coordinates for {City}: {Latitude}, {Longitude}", city, latitude, longitude);
                    return (latitude, longitude);
                }
                _logger.LogWarning("Couldn't find {City} on the map", city);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong looking up {City}", city);
                throw;
            }
        }
    }
}