using WeatherApp.ViewModels;

namespace WeatherApp.Views;

public partial class FavoritesPage : ContentPage
{
	private readonly FavoritesViewModel _viewModel;

	public FavoritesPage(FavoritesViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.InitializeAsync();
	}
}
