using Microsoft.Maui.Devices.Sensors;

namespace WeatherApp.Services
{
    /// <summary>
    /// Service for handling device location and permissions
    /// </summary>
    public class LocationService : ILocationService
    {
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var hasPermission = await CheckPermissionsAsync();
                if (!hasPermission)
                {
                    hasPermission = await RequestPermissionsAsync();
                }

                if (!hasPermission)
                {
                    return null;
                }

                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });

                return location;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CheckPermissionsAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
    }
}
