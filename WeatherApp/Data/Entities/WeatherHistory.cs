using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Data.Entities
{
    /// <summary>
    /// Entity for caching weather data and tracking history
    /// </summary>
    public class WeatherHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        [MaxLength(200)]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Weather data stored as JSON for flexible querying
        /// </summary>
        [Required]
        public string WeatherDataJson { get; set; } = string.Empty;

        /// <summary>
        /// Quick access fields for queries (denormalized for performance)
        /// </summary>
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string? Condition { get; set; }

        /// <summary>
        /// TTL - data is considered fresh for 10 minutes
        /// </summary>
        public bool IsStale => DateTime.UtcNow - FetchedAt > TimeSpan.FromMinutes(10);
    }
}
