using System.Collections.ObjectModel;
using System.Windows.Input;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    /// <summary>
    /// ViewModel for the main weather display page
    /// Implements MVVM pattern with proper separation of concerns
    /// </summary>
    public class WeatherViewModel : BaseViewModel
    {
        private readonly IWeatherService _weatherService;
        private readonly ILocationService _locationService;
        private readonly IFavoritesRepository _favoritesRepo;
        private readonly IWeatherCacheService _cacheService;
        
        private WeatherData? _weatherData;
        private string _searchQuery = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isRefreshing;
        private bool _isFavorite;

        public WeatherViewModel(IWeatherService weatherService, ILocationService locationService, IFavoritesRepository favoritesRepo, IWeatherCacheService cacheService)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _favoritesRepo = favoritesRepo ?? throw new ArgumentNullException(nameof(favoritesRepo));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            
            Title = "Weather";
            
            RefreshCommand = new Command(async () => await RefreshWeatherAsync());
            SearchCommand = new Command(async () => await SearchLocationAsync());
            GetCurrentLocationCommand = new Command(async () => await LoadCurrentLocationWeatherAsync());
            ToggleFavoriteCommand = new Command(async () => await ToggleFavoriteAsync());
        }

        public WeatherData? WeatherData
        {
            get => _weatherData;
            set => SetProperty(ref _weatherData, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GetCurrentLocationCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        public async Task InitializeAsync()
        {
            // Try to load last viewed weather from cache
            var cachedWeather = await _cacheService.GetLastViewedWeatherAsync();
            if (cachedWeather != null)
            {
                WeatherData = cachedWeather;
                await CheckIfFavoriteAsync();
            }
        }

        private async Task LoadCurrentLocationWeatherAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var location = await _locationService.GetCurrentLocationAsync();
                
                if (location == null)
                {
                    ErrorMessage = "Unable to get your location. Please search for a city.";
                    return;
                }

                // Try to get from cache first
                var cachedWeather = await _cacheService.GetCachedWeatherAsync(location.Latitude, location.Longitude);
                if (cachedWeather != null)
                {
                    WeatherData = cachedWeather;
                    await CheckIfFavoriteAsync();
                    return;
                }

                var weather = await _weatherService.GetWeatherAsync(location.Latitude, location.Longitude);
                
                if (weather == null)
                {
                    ErrorMessage = "Unable to fetch weather data. Please try again.";
                    return;
                }

                WeatherData = weather;
                await _cacheService.SaveWeatherCacheAsync(weather);
                await _cacheService.SaveLastViewedLocationAsync(location.Latitude, location.Longitude);
                await CheckIfFavoriteAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading weather: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SearchLocationAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(SearchQuery)) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // First try to find the location
                var locations = await _weatherService.SearchLocationsAsync(SearchQuery);
                if (locations == null || !locations.Any())
                {
                    ErrorMessage = "Location not found. Please try a different search.";
                    return;
                }

                var firstLocation = locations.First();
                
                // Try to get from cache first
                var cachedWeather = await _cacheService.GetCachedWeatherAsync(firstLocation.Latitude, firstLocation.Longitude);
                if (cachedWeather != null)
                {
                    WeatherData = cachedWeather;
                    await CheckIfFavoriteAsync();
                    return;
                }

                var weather = await _weatherService.GetWeatherByLocationAsync(SearchQuery);
                
                if (weather == null)
                {
                    ErrorMessage = "Location not found. Please try a different search.";
                    return;
                }

                WeatherData = weather;
                await _cacheService.SaveWeatherCacheAsync(weather);
                await _cacheService.SaveLastViewedLocationAsync(weather.Latitude, weather.Longitude);
                await CheckIfFavoriteAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error searching location: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshWeatherAsync()
        {
            if (WeatherData == null)
            {
                await LoadCurrentLocationWeatherAsync();
                return;
            }

            try
            {
                IsRefreshing = true;
                ErrorMessage = string.Empty;

                var weather = await _weatherService.GetWeatherAsync(
                    WeatherData.Latitude, 
                    WeatherData.Longitude);
                
                if (weather != null)
                {
                    WeatherData = weather;
                    await _cacheService.SaveWeatherCacheAsync(weather);
                    await _cacheService.SaveLastViewedLocationAsync(weather.Latitude, weather.Longitude);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error refreshing: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error refreshing weather: {ex}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task LoadWeatherByCoordinatesAsync(double latitude, double longitude, string? locationName = null)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Try to get from cache first
                var cachedWeather = await _cacheService.GetCachedWeatherAsync(latitude, longitude);
                if (cachedWeather != null)
                {
                    WeatherData = cachedWeather;
                    await CheckIfFavoriteAsync();
                    IsBusy = false;
                    return;
                }

                var weather = await _weatherService.GetWeatherAsync(latitude, longitude);
                
                if (weather == null)
                {
                    ErrorMessage = "Unable to fetch weather data. Please try again.";
                    return;
                }

                // If location name wasn't provided, it will be in the weather data
                WeatherData = weather;
                await _cacheService.SaveWeatherCacheAsync(weather);
                await _cacheService.SaveLastViewedLocationAsync(latitude, longitude);
                await CheckIfFavoriteAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading weather: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading weather by coordinates: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ToggleFavoriteAsync()
        {
            if (WeatherData == null) return;

            try
            {
                var existing = await _favoritesRepo.GetByCoordinatesAsync(
                    WeatherData.Latitude, 
                    WeatherData.Longitude);

                if (existing != null)
                {
                    // Remove from favorites
                    await _favoritesRepo.DeleteAsync(existing.Id);
                    IsFavorite = false;
                    await Shell.Current.DisplayAlertAsync("Removed", $"{WeatherData.LocationName} removed from favorites", "OK");
                }
                else
                {
                    // Add to favorites
                    var favorite = new Data.Entities.FavoriteLocation
                    {
                        Name = WeatherData.LocationName ?? "Unknown",
                        Region = WeatherData.Country ?? string.Empty,
                        Country = WeatherData.Country ?? string.Empty,
                        Latitude = WeatherData.Latitude,
                        Longitude = WeatherData.Longitude,
                        LastKnownTemperature = WeatherData.Current?.Temperature ?? 0,
                        LastKnownCondition = WeatherData.Current?.Condition ?? string.Empty
                    };
                    await _favoritesRepo.AddAsync(favorite);
                    IsFavorite = true;
                    await Shell.Current.DisplayAlertAsync("Added", $"{WeatherData.LocationName} added to favorites", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to update favorites: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Error toggling favorite: {ex}");
            }
        }

        private async Task CheckIfFavoriteAsync()
        {
            if (WeatherData == null) return;

            try
            {
                var existing = await _favoritesRepo.GetByCoordinatesAsync(
                    WeatherData.Latitude,
                    WeatherData.Longitude);
                IsFavorite = existing != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking favorite status: {ex}");
            }
        }
    }
}
