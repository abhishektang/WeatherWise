using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Services;
using WeatherApp.ViewModels;
using WeatherApp.Views;

namespace WeatherApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register Entity Framework Core with SQLite
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "weather.db");
		builder.Services.AddDbContext<WeatherDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		// Register Services
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<IWeatherService, WeatherService>();
		builder.Services.AddSingleton<ILocationService, LocationService>();
		builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
		builder.Services.AddScoped<IWeatherCacheService, WeatherCacheService>();

		// Register ViewModels as Singleton to persist data across navigation
		builder.Services.AddSingleton<WeatherViewModel>();
		builder.Services.AddSingleton<FavoritesViewModel>();

		// Register Views as Singleton to preserve state
		builder.Services.AddSingleton<WeatherPage>();
		builder.Services.AddSingleton<FavoritesPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Initialize database
		using (var scope = app.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
			dbContext.Database.EnsureCreated();
		}

		return app;
	}
}
