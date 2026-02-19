using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Data.Entities;

namespace WeatherApp.Services
{
    public class FavoritesRepository : IFavoritesRepository
    {
        private readonly WeatherDbContext _context;

        public FavoritesRepository(WeatherDbContext context)
        {
            _context = context;
        }

        public async Task<List<FavoriteLocation>> GetAllAsync()
        {
            // Get all favorites ordered by most recently accessed
            return await _context.FavoriteLocations
                .OrderByDescending(f => f.LastAccessedDate ?? f.AddedDate)
                .ToListAsync();
        }

        public async Task<FavoriteLocation?> GetByIdAsync(int id)
        {
            return await _context.FavoriteLocations.FindAsync(id);
        }

        public async Task<FavoriteLocation?> GetByCoordinatesAsync(double latitude, double longitude)
        {
            // Find location within 0.01 degree tolerance (~1km)
            return await _context.FavoriteLocations
                .FirstOrDefaultAsync(f => 
                    Math.Abs(f.Latitude - latitude) < 0.01 && 
                    Math.Abs(f.Longitude - longitude) < 0.01);
        }

        public async Task<FavoriteLocation> AddAsync(FavoriteLocation location)
        {
            _context.FavoriteLocations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task UpdateAsync(FavoriteLocation location)
        {
            _context.FavoriteLocations.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var location = await GetByIdAsync(id);
            if (location != null)
            {
                _context.FavoriteLocations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RecordAccessAsync(int id)
        {
            var location = await GetByIdAsync(id);
            if (location != null)
            {
                location.LastAccessedDate = DateTime.UtcNow;
                location.AccessCount++;
                await UpdateAsync(location);
            }
        }
    }
}
