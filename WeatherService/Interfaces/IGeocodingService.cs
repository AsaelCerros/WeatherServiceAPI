namespace WeatherService.Interfaces
{
    public interface IGeocodingService
    {
        Task<(double Latitude, double Longitude)?> GetCoordinatesForCityAsync(string city);
    }
}