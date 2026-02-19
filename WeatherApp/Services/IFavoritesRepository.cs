using WeatherApp.Data.Entities;

namespace WeatherApp.Services
{
    /// <summary>
    /// Repository for managing favorite locations using Entity Framework
    /// </summary>
    public interface IFavoritesRepository
    {
        Task<List<FavoriteLocation>> GetAllAsync();
        Task<FavoriteLocation?> GetByIdAsync(int id);
        Task<FavoriteLocation?> GetByCoordinatesAsync(double latitude, double longitude);
        Task<FavoriteLocation> AddAsync(FavoriteLocation location);
        Task UpdateAsync(FavoriteLocation location);
        Task DeleteAsync(int id);
        Task RecordAccessAsync(int id);
    }
}
