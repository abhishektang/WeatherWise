using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Data.Entities
{
    /// <summary>
    /// Entity tracking user's search history
    /// </summary>
    public class SearchHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string SearchQuery { get; set; } = string.Empty;

        [Required]
        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// If user selected a result, store its details
        /// </summary>
        public string? SelectedLocationName { get; set; }
        public double? SelectedLatitude { get; set; }
        public double? SelectedLongitude { get; set; }
    }
}
