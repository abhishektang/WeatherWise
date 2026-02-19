using System.Collections.ObjectModel;
using System.Windows.Input;
using WeatherApp.Data.Entities;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    public class FavoritesViewModel : BaseViewModel
    {
        private readonly IFavoritesRepository _favoritesRepo;
        private readonly IWeatherService _weatherService;
        private readonly IWeatherCacheService _cacheService;
        private string _errorMessage = string.Empty;

        public ObservableCollection<FavoriteLocation> Favorites { get; } = new();

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoadFavoritesCommand { get; }
        public ICommand SelectFavoriteCommand { get; }
        public ICommand DeleteFavoriteCommand { get; }
        public ICommand RefreshWeatherCommand { get; }

        public FavoritesViewModel(IFavoritesRepository favoritesRepo, IWeatherService weatherService, IWeatherCacheService cacheService)
        {
            _favoritesRepo = favoritesRepo;
            _weatherService = weatherService;
            _cacheService = cacheService;

            Title = "Favorite Locations";

            LoadFavoritesCommand = new Command(async () => await LoadFavoritesAsync());
            SelectFavoriteCommand = new Command<FavoriteLocation>(async (fav) => await SelectFavoriteAsync(fav));
            DeleteFavoriteCommand = new Command<FavoriteLocation>(async (fav) => await DeleteFavoriteAsync(fav));
            RefreshWeatherCommand = new Command<FavoriteLocation>(async (fav) => await RefreshWeatherAsync(fav));
        }

        private async Task LoadFavoritesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var favorites = await _favoritesRepo.GetAllAsync();
                
                Favorites.Clear();
                foreach (var fav in favorites)
                {
                    Favorites.Add(fav);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading favorites: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error in LoadFavoritesAsync: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SelectFavoriteAsync(FavoriteLocation? favorite)
        {
            if (favorite == null) return;

            try
            {
                await _favoritesRepo.RecordAccessAsync(favorite.Id);
                
                // Navigate to weather page with this location
                await Shell.Current.GoToAsync($"//WeatherPage?lat={favorite.Latitude}&lon={favorite.Longitude}&name={Uri.EscapeDataString(favorite.Name)}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error selecting favorite: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error in SelectFavoriteAsync: {ex}");
            }
        }

        private async Task DeleteFavoriteAsync(FavoriteLocation? favorite)
        {
            if (favorite == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Delete Favorite",
                $"Remove {favorite.Name} from favorites?",
                "Delete",
                "Cancel");

            if (!confirm) return;

            try
            {
                await _favoritesRepo.DeleteAsync(favorite.Id);
                Favorites.Remove(favorite);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting favorite: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error in DeleteFavoriteAsync: {ex}");
            }
        }

        private async Task RefreshWeatherAsync(FavoriteLocation? favorite)
        {
            if (favorite == null) return;

            try
            {
                var weatherData = await _weatherService.GetWeatherAsync(favorite.Latitude, favorite.Longitude);
                
                if (weatherData?.Current != null)
                {
                    favorite.LastKnownTemperature = weatherData.Current.Temperature;
                    favorite.LastKnownCondition = weatherData.Current.Condition;
                    await _favoritesRepo.UpdateAsync(favorite);
                    
                    // Update the cache as well
                    await _cacheService.SaveWeatherCacheAsync(weatherData);
                    
                    // Refresh the list
                    var index = Favorites.IndexOf(favorite);
                    if (index >= 0)
                    {
                        Favorites[index] = favorite;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing weather: {ex}");
            }
        }

        public async Task InitializeAsync()
        {
            await LoadFavoritesAsync();
        }
    }
}
