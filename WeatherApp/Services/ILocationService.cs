namespace WeatherApp.Services
{
    /// <summary>
    /// Interface for location services (GPS, geocoding)
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// Gets the current device location
        /// </summary>
        Task<Microsoft.Maui.Devices.Sensors.Location?> GetCurrentLocationAsync();

        /// <summary>
        /// Checks if location permissions are granted
        /// </summary>
        Task<bool> CheckPermissionsAsync();

        /// <summary>
        /// Requests location permissions from the user
        /// </summary>
        Task<bool> RequestPermissionsAsync();
    }
}
