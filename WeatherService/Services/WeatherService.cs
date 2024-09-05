using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.Json;
using WeatherService.Interfaces;
using WeatherService.Models;

namespace WeatherService.Services
{
    public class WeatherService: IWeatherService
    {
        private readonly IMongoCollection<WeatherData> _weatherCollection;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherService> _logger;
        private const string OpenMeteoApiUrl = "https://api.open-meteo.com/v1/forecast";

        // Constructor sets up MongoDB connection and HTTP client
        public WeatherService(
            IOptions<WeatherDatabaseSettings> weatherDatabaseSettings,
            IHttpClientFactory httpClientFactory,
            ILogger<WeatherService> logger)
        {
            // Connect to MongoDB and get the weather collection
            var mongoClient = new MongoClient(weatherDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(weatherDatabaseSettings.Value.DatabaseName);
            _weatherCollection = mongoDatabase.GetCollection<WeatherData>(
                weatherDatabaseSettings.Value.WeatherCollectionName);
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<WeatherData> GetWeatherDataAsync(double latitude, double longitude)
        {
            try
            {
                // Check if we have recent data in the database
                var existingData = await _weatherCollection
                    .Find(w => w.Latitude == latitude && w.Longitude == longitude && w.Timestamp > DateTime.UtcNow.AddHours(-1))
                    .FirstOrDefaultAsync();

                if (existingData != null)
                {
                    _logger.LogInformation("Found cached weather data for {Latitude}, {Longitude}", latitude, longitude);
                    return existingData;
                }

                // If no recent data, fetch new data from the API
                var newData = await FetchWeatherDataFromApiAsync(latitude, longitude);
                await _weatherCollection.InsertOneAsync(newData);
                _logger.LogInformation("Fetched fresh weather data for {Latitude}, {Longitude}", latitude, longitude);
                return newData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oops! Something went wrong getting weather data for {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
        }

        private async Task<WeatherData> FetchWeatherDataFromApiAsync(double latitude, double longitude)
        {
            try
            {
                // Build the URL for the API request
                var url = $"{OpenMeteoApiUrl}?latitude={latitude}&longitude={longitude}&current_weather=true&daily=sunrise&timezone=auto";
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;

                // Extract the relevant data from the JSON response
                var weatherData = new WeatherData
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    Temperature = root.GetProperty("current_weather").GetProperty("temperature").GetDouble(),
                    WindDirection = root.GetProperty("current_weather").GetProperty("winddirection").GetDouble(),
                    WindSpeed = root.GetProperty("current_weather").GetProperty("windspeed").GetDouble(),
                    Sunrise = DateTime.Parse(root.GetProperty("daily").GetProperty("sunrise").EnumerateArray().First().GetString()!),
                    Timestamp = DateTime.UtcNow
                };

                return weatherData;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Couldn't reach the Open-Meteo API for {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "The Open-Meteo API returned some funky JSON for {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
        }
    }
}