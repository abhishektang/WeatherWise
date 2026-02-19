using WeatherApp.ViewModels;

namespace WeatherApp.Views
{
    [QueryProperty(nameof(Latitude), "lat")]
    [QueryProperty(nameof(Longitude), "lon")]
    [QueryProperty(nameof(LocationName), "name")]
    public partial class WeatherPage : ContentPage
    {
        private readonly WeatherViewModel _viewModel;
        private double? _latitude;
        private double? _longitude;
        private string? _locationName;

        public string Latitude
        {
            set
            {
                if (double.TryParse(value, out var lat))
                    _latitude = lat;
            }
        }

        public string Longitude
        {
            set
            {
                if (double.TryParse(value, out var lon))
                    _longitude = lon;
            }
        }

        public string LocationName
        {
            set => _locationName = value;
        }

        public WeatherPage(WeatherViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // If we have lat/lon from navigation, load weather for that location
            if (_latitude.HasValue && _longitude.HasValue)
            {
                await _viewModel.LoadWeatherByCoordinatesAsync(_latitude.Value, _longitude.Value, _locationName);
                // Reset the parameters after loading
                _latitude = null;
                _longitude = null;
                _locationName = null;
            }
            else
            {
                await _viewModel.InitializeAsync();
            }
        }
    }
}
