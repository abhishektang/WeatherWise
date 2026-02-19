using Microsoft.EntityFrameworkCore;
using WeatherApp.Data.Entities;

namespace WeatherApp.Data
{
    /// <summary>
    /// Database context for weather app
    /// </summary>
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
            : base(options)
        {
        }

        public DbSet<FavoriteLocation> FavoriteLocations { get; set; }
        public DbSet<WeatherHistory> WeatherHistory { get; set; }
        public DbSet<SearchHistory> SearchHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for better query performance
            modelBuilder.Entity<FavoriteLocation>()
                .HasIndex(f => f.AddedDate);

            modelBuilder.Entity<FavoriteLocation>()
                .HasIndex(f => new { f.Latitude, f.Longitude });

            modelBuilder.Entity<WeatherHistory>()
                .HasIndex(w => new { w.Latitude, w.Longitude, w.FetchedAt });

            modelBuilder.Entity<WeatherHistory>()
                .HasIndex(w => w.FetchedAt);

            modelBuilder.Entity<SearchHistory>()
                .HasIndex(s => s.SearchedAt);
        }
    }
}
