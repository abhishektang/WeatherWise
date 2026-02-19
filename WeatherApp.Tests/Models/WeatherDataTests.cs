using FluentAssertions;
using WeatherApp.Models;

namespace WeatherApp.Tests.Models;

public class WeatherDataTests
{
    [Fact] // Positive Test: Verifies basic object creation with valid data
    public void WeatherData_CanBeCreated()
    {
        // Arrange & Act
        var weatherData = new WeatherData
        {
            LocationName = "San Francisco",
            Country = "United States",
            Latitude = 37.7749,
            Longitude = -122.4194,
            LastUpdated = DateTime.Now
        };

        // Assert
        weatherData.LocationName.Should().Be("San Francisco");
        weatherData.Country.Should().Be("United States");
        weatherData.Latitude.Should().Be(37.7749);
        weatherData.Longitude.Should().Be(-122.4194);
    }

    [Fact] // Positive Test: Verifies CurrentWeather object creation with typical values
    public void CurrentWeather_CanBeCreated()
    {
        // Arrange & Act
        var current = new CurrentWeather
        {
            Temperature = 20.5,
            FeelsLike = 19.0,
            Humidity = 65,
            Pressure = 1013.25,
            WindSpeed = 5.5,
            WindDegree = 180,
            CloudCover = 40,
            Visibility = 10,
            Condition = "Partly cloudy",
            ConditionIcon = "02d"
        };

        // Assert
        current.Temperature.Should().Be(20.5);
        current.Humidity.Should().Be(65);
        current.Condition.Should().Be("Partly cloudy");
    }

    [Fact] // Positive Test: Verifies HourlyForecast object creation with standard data
    public void HourlyForecast_CanBeCreated()
    {
        // Arrange & Act
        var hourly = new HourlyForecast
        {
            DateTime = DateTime.Now,
            Temperature = 21.0,
            FeelsLike = 20.0,
            Condition = "Clear",
            Humidity = 60,
            WindSpeed = 5.0,
            ChanceOfRain = 10
        };

        // Assert
        hourly.Temperature.Should().Be(21.0);
        hourly.ChanceOfRain.Should().Be(10);
    }

    [Fact] // Positive Test: Verifies DailyForecast object creation with normal values
    public void DailyForecast_CanBeCreated()
    {
        // Arrange & Act
        var daily = new DailyForecast
        {
            Date = DateTime.Today,
            MaxTemperature = 25.0,
            MinTemperature = 15.0,
            AvgTemperature = 20.0,
            Condition = "Sunny",
            ChanceOfRain = 5
        };

        // Assert
        daily.MaxTemperature.Should().Be(25.0);
        daily.MinTemperature.Should().Be(15.0);
        daily.AvgTemperature.Should().Be(20.0);
    }

    [Fact] // Positive Test: Verifies Location object creation with valid coordinates
    public void Location_CanBeCreated()
    {
        // Arrange & Act
        var location = new Location
        {
            Name = "San Francisco",
            Region = "California",
            Country = "United States",
            Latitude = 37.7749,
            Longitude = -122.4194
        };

        // Assert
        location.Name.Should().Be("San Francisco");
        location.Region.Should().Be("California");
        location.Country.Should().Be("United States");
    }

    [Fact] // Negative Test: Verifies handling of null/missing optional data
    public void WeatherData_WithNullValues_StoresCorrectly()
    {
        // Arrange & Act
        var weatherData = new WeatherData
        {
            LocationName = "Unknown",
            Country = null,
            Latitude = 0.0,
            Longitude = 0.0,
            Current = null,
            HourlyForecasts = null,
            DailyForecasts = null
        };

        // Assert
        weatherData.LocationName.Should().Be("Unknown");
        weatherData.Country.Should().BeNull();
        weatherData.Current.Should().BeNull();
    }

    [Fact] // Edge Case Test: Verifies handling of negative temperature values
    public void CurrentWeather_WithNegativeTemperature_StoresCorrectly()
    {
        // Arrange & Act
        var current = new CurrentWeather
        {
            Temperature = -15.5,
            FeelsLike = -20.0,
            Humidity = 85,
            Pressure = 1025.0,
            WindSpeed = 25.0,
            Condition = "Snow"
        };

        // Assert
        current.Temperature.Should().Be(-15.5);
        current.FeelsLike.Should().Be(-20.0);
    }

    [Fact] // Edge Case Test: Verifies handling of extreme weather conditions (max values)
    public void CurrentWeather_WithExtremeValues_StoresCorrectly()
    {
        // Arrange & Act
        var current = new CurrentWeather
        {
            Temperature = 50.0, // Extreme heat
            Humidity = 100, // Max humidity
            WindSpeed = 150.0, // Hurricane force
            CloudCover = 100, // Complete overcast
            Visibility = 0 // Zero visibility
        };

        // Assert
        current.Temperature.Should().Be(50.0);
        current.Humidity.Should().Be(100);
        current.WindSpeed.Should().Be(150.0);
    }

    [Fact] // Edge Case Test: Verifies handling of zero probability (minimum boundary)
    public void HourlyForecast_WithZeroChanceOfRain_StoresCorrectly()
    {
        // Arrange & Act
        var hourly = new HourlyForecast
        {
            DateTime = DateTime.Now,
            Temperature = 25.0,
            ChanceOfRain = 0
        };

        // Assert
        hourly.ChanceOfRain.Should().Be(0);
    }

    [Fact] // Edge Case Test: Verifies handling of 100% probability (maximum boundary)
    public void HourlyForecast_WithMaxChanceOfRain_StoresCorrectly()
    {
        // Arrange & Act
        var hourly = new HourlyForecast
        {
            DateTime = DateTime.Now,
            Temperature = 18.0,
            ChanceOfRain = 100
        };

        // Assert
        hourly.ChanceOfRain.Should().Be(100);
    }

    [Fact] // Positive Test: Verifies logical temperature range (min < avg < max)
    public void DailyForecast_MinTemperature_CanBeLowerThanMax()
    {
        // Arrange & Act
        var daily = new DailyForecast
        {
            Date = DateTime.Today,
            MaxTemperature = 10.0,
            MinTemperature = -5.0,
            AvgTemperature = 2.5,
            Condition = "Cold"
        };

        // Assert
        daily.MinTemperature.Should().BeLessThan(daily.MaxTemperature);
        daily.AvgTemperature.Should().BeGreaterThan(daily.MinTemperature);
        daily.AvgTemperature.Should().BeLessThan(daily.MaxTemperature);
    }

    [Fact] // Edge Case Test: Verifies handling of negative latitude/longitude values
    public void Location_WithNegativeCoordinates_StoresCorrectly()
    {
        // Arrange & Act - Southern hemisphere and western longitude
        var location = new Location
        {
            Name = "Sydney",
            Country = "Australia",
            Latitude = -33.8688,
            Longitude = 151.2093
        };

        // Assert
        location.Latitude.Should().BeNegative();
        location.Longitude.Should().BePositive();
    }
}
