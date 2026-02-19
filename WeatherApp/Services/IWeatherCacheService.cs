using WeatherApp.Models;

namespace WeatherApp.Services
{
    public interface IWeatherCacheService
    {
        Task<WeatherData?> GetCachedWeatherAsync(double latitude, double longitude);
        Task SaveWeatherCacheAsync(WeatherData weatherData);
        Task<WeatherData?> GetLastViewedWeatherAsync();
        Task SaveLastViewedLocationAsync(double latitude, double longitude);
    }
}
