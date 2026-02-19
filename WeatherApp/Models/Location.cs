namespace WeatherApp.Models
{
    /// <summary>
    /// Represents a geographical location
    /// </summary>
    public class Location
    {
        public string? Name { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? TimeZone { get; set; }
        public bool IsFavorite { get; set; }

        public string DisplayName => !string.IsNullOrEmpty(Region) 
            ? $"{Name}, {Region}, {Country}" 
            : $"{Name}, {Country}";
    }
}
