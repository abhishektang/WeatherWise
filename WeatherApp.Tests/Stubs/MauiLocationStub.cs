namespace Microsoft.Maui.Devices.Sensors
{
    /// <summary>
    /// Stub Location class for testing purposes (replaces MAUI dependency)
    /// </summary>
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Accuracy { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
