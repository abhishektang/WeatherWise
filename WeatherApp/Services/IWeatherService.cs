using WeatherApp.Models;

namespace WeatherApp.Services
{
    /// <summary>
    /// Interface for weather data retrieval service
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Gets current weather and forecasts for a location
        /// </summary>
        Task<WeatherData?> GetWeatherAsync(double latitude, double longitude);

        /// <summary>
        /// Gets current weather and forecasts by location name
        /// </summary>
        Task<WeatherData?> GetWeatherByLocationAsync(string locationName);

        /// <summary>
        /// Searches for locations by name
        /// </summary>
        Task<List<Models.Location>?> SearchLocationsAsync(string query);
    }
}
