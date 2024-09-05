using WeatherService.Models;

namespace WeatherService.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherData> GetWeatherDataAsync(double latitude, double longitude);
    }
}