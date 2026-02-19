using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Data.Entities
{
    /// <summary>
    /// Entity representing a user's favorite location
    /// </summary>
    public class FavoriteLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Region { get; set; }

        public string? Country { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastAccessedDate { get; set; }

        public int AccessCount { get; set; } = 0;

        /// <summary>
        /// Optional: Store the last known temperature for quick display
        /// </summary>
        public double? LastKnownTemperature { get; set; }

        /// <summary>
        /// Optional: Store the last known weather condition
        /// </summary>
        public string? LastKnownCondition { get; set; }
    }
}
