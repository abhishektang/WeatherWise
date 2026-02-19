using WeatherApp.Views;

namespace WeatherApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute("WeatherPage", typeof(WeatherPage));
	}
}
