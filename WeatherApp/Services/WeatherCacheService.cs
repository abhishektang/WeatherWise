using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Data.Entities;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class WeatherCacheService : IWeatherCacheService
    {
        private readonly WeatherDbContext _dbContext;
        private const string LastLocationKey = "last_location";

        public WeatherCacheService(WeatherDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<WeatherData?> GetCachedWeatherAsync(double latitude, double longitude)
        {
            try
            {
                // Find cached weather within 0.01 degree tolerance (approximately 1km)
                var cached = await _dbContext.WeatherHistory
                    .Where(w => Math.Abs(w.Latitude - latitude) < 0.01 && 
                               Math.Abs(w.Longitude - longitude) < 0.01)
                    .OrderByDescending(w => w.FetchedAt)
                    .FirstOrDefaultAsync();

                if (cached == null || cached.IsStale)
                {
                    return null;
                }

                // Deserialize the cached JSON back to WeatherData
                if (!string.IsNullOrEmpty(cached.WeatherDataJson))
                {
                    var weatherData = JsonSerializer.Deserialize<WeatherData>(cached.WeatherDataJson);
                    return weatherData;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cached weather: {ex.Message}");
                return null;
            }
        }

        public async Task SaveWeatherCacheAsync(WeatherData weatherData)
        {
            try
            {
                // Serialize the entire WeatherData object to JSON
                var json = JsonSerializer.Serialize(weatherData);

                var cached = new WeatherHistory
                {
                    Latitude = weatherData.Latitude,
                    Longitude = weatherData.Longitude,
                    LocationName = weatherData.LocationName ?? "Unknown",
                    FetchedAt = DateTime.UtcNow,
                    WeatherDataJson = json,
                    Temperature = weatherData.Current?.Temperature ?? 0,
                    FeelsLike = weatherData.Current?.FeelsLike ?? 0,
                    Humidity = weatherData.Current?.Humidity ?? 0,
                    WindSpeed = weatherData.Current?.WindSpeed ?? 0,
                    Condition = weatherData.Current?.Condition ?? "Unknown"
                };

                _dbContext.WeatherHistory.Add(cached);
                await _dbContext.SaveChangesAsync();

                // Clean up old cache entries (keep only last 50)
                var oldEntries = await _dbContext.WeatherHistory
                    .OrderByDescending(w => w.FetchedAt)
                    .Skip(50)
                    .ToListAsync();

                if (oldEntries.Any())
                {
                    _dbContext.WeatherHistory.RemoveRange(oldEntries);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving weather cache: {ex.Message}");
            }
        }

        public async Task<WeatherData?> GetLastViewedWeatherAsync()
        {
            try
            {
                // Get the last viewed location from preferences
                var lastLat = Preferences.Get($"{LastLocationKey}_lat", double.MinValue);
                var lastLon = Preferences.Get($"{LastLocationKey}_lon", double.MinValue);

                if (lastLat == double.MinValue || lastLon == double.MinValue)
                {
                    return null;
                }

                // Try to get cached weather for that location
                return await GetCachedWeatherAsync(lastLat, lastLon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting last viewed weather: {ex.Message}");
                return null;
            }
        }

        public async Task SaveLastViewedLocationAsync(double latitude, double longitude)
        {
            try
            {
                Preferences.Set($"{LastLocationKey}_lat", latitude);
                Preferences.Set($"{LastLocationKey}_lon", longitude);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving last viewed location: {ex.Message}");
            }
        }
    }
}
