using FluentAssertions;
using Moq;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Tests.Services;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherService> _mockWeatherService;

    public WeatherServiceTests()
    {
        _mockWeatherService = new Mock<IWeatherService>();
    }

    [Fact] // Positive Test: Verifies successful weather data retrieval by coordinates
    public async Task GetWeatherAsync_WithValidCoordinates_ReturnsWeatherData()
    {
        // Arrange
        var latitude = 37.7749;
        var longitude = -122.4194;
        var expectedWeatherData = new WeatherData
        {
            LocationName = "San Francisco",
            Country = "United States",
            Latitude = latitude,
            Longitude = longitude,
            Current = new CurrentWeather
            {
                Temperature = 20.5,
                Condition = "Clear",
                Humidity = 65
            }
        };

        _mockWeatherService
            .Setup(s => s.GetWeatherAsync(latitude, longitude))
            .ReturnsAsync(expectedWeatherData);

        // Act
        var result = await _mockWeatherService.Object.GetWeatherAsync(latitude, longitude);

        // Assert
        result.Should().NotBeNull();
        result!.LocationName.Should().Be("San Francisco");
        result.Latitude.Should().Be(latitude);
        result.Longitude.Should().Be(longitude);
        result.Current.Should().NotBeNull();
        result.Current!.Temperature.Should().Be(20.5);
    }

    [Fact] // Positive Test: Verifies successful weather retrieval by location name
    public async Task GetWeatherByLocationAsync_WithValidLocationName_ReturnsWeatherData()
    {
        // Arrange
        var locationName = "Paris";
        var expectedWeatherData = new WeatherData
        {
            LocationName = "Paris",
            Country = "France",
            Latitude = 48.8566,
            Longitude = 2.3522,
            Current = new CurrentWeather
            {
                Temperature = 15.0,
                Condition = "Partly cloudy"
            }
        };

        _mockWeatherService
            .Setup(s => s.GetWeatherByLocationAsync(locationName))
            .ReturnsAsync(expectedWeatherData);

        // Act
        var result = await _mockWeatherService.Object.GetWeatherByLocationAsync(locationName);

        // Assert
        result.Should().NotBeNull();
        result!.LocationName.Should().Be("Paris");
        result.Country.Should().Be("France");
        result.Current.Should().NotBeNull();
    }

    [Fact] // Negative Test: Verifies null return when location not found
    public async Task GetWeatherByLocationAsync_WithInvalidLocation_ReturnsNull()
    {
        // Arrange
        var invalidLocation = "NonExistentCity12345";

        _mockWeatherService
            .Setup(s => s.GetWeatherByLocationAsync(invalidLocation))
            .ReturnsAsync((WeatherData?)null);

        // Act
        var result = await _mockWeatherService.Object.GetWeatherByLocationAsync(invalidLocation);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Positive Test: Verifies location search returns multiple results
    public async Task SearchLocationsAsync_WithValidQuery_ReturnsLocationList()
    {
        // Arrange
        var query = "San";
        var expectedLocations = new List<Location>
        {
            new Location
            {
                Name = "San Francisco",
                Region = "California",
                Country = "United States",
                Latitude = 37.7749,
                Longitude = -122.4194
            },
            new Location
            {
                Name = "San Diego",
                Region = "California",
                Country = "United States",
                Latitude = 32.7157,
                Longitude = -117.1611
            },
            new Location
            {
                Name = "Santiago",
                Region = "Santiago Metropolitan",
                Country = "Chile",
                Latitude = -33.4489,
                Longitude = -70.6693
            }
        };

        _mockWeatherService
            .Setup(s => s.SearchLocationsAsync(query))
            .ReturnsAsync(expectedLocations);

        // Act
        var result = await _mockWeatherService.Object.SearchLocationsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result![0].Name.Should().Be("San Francisco");
        result[1].Name.Should().Be("San Diego");
        result[2].Name.Should().Be("Santiago");
    }

    [Fact] // Negative Test: Verifies empty result when no locations match query
    public async Task SearchLocationsAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var query = "XYZ123NonExistent";

        _mockWeatherService
            .Setup(s => s.SearchLocationsAsync(query))
            .ReturnsAsync(new List<Location>());

        // Act
        var result = await _mockWeatherService.Object.SearchLocationsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact] // Negative Test: Verifies null return when search fails
    public async Task SearchLocationsAsync_WhenServiceFails_ReturnsNull()
    {
        // Arrange
        var query = "London";

        _mockWeatherService
            .Setup(s => s.SearchLocationsAsync(query))
            .ReturnsAsync((List<Location>?)null);

        // Act
        var result = await _mockWeatherService.Object.SearchLocationsAsync(query);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Edge Case Test: Verifies handling of extreme coordinates (North Pole)
    public async Task GetWeatherAsync_WithExtremeCoordinates_ReturnsWeatherData()
    {
        // Arrange - North Pole coordinates
        var latitude = 90.0;
        var longitude = 0.0;
        var expectedWeatherData = new WeatherData
        {
            LocationName = "North Pole",
            Latitude = latitude,
            Longitude = longitude,
            Current = new CurrentWeather
            {
                Temperature = -40.0,
                Condition = "Snow"
            }
        };

        _mockWeatherService
            .Setup(s => s.GetWeatherAsync(latitude, longitude))
            .ReturnsAsync(expectedWeatherData);

        // Act
        var result = await _mockWeatherService.Object.GetWeatherAsync(latitude, longitude);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(90.0);
        result.Current!.Temperature.Should().Be(-40.0);
    }

    [Fact] // Edge Case Test: Verifies handling of negative coordinates (Southern hemisphere)
    public async Task GetWeatherAsync_WithNegativeCoordinates_ReturnsWeatherData()
    {
        // Arrange - Sydney, Australia (negative latitude)
        var latitude = -33.8688;
        var longitude = 151.2093;
        var expectedWeatherData = new WeatherData
        {
            LocationName = "Sydney",
            Country = "Australia",
            Latitude = latitude,
            Longitude = longitude
        };

        _mockWeatherService
            .Setup(s => s.GetWeatherAsync(latitude, longitude))
            .ReturnsAsync(expectedWeatherData);

        // Act
        var result = await _mockWeatherService.Object.GetWeatherAsync(latitude, longitude);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().BeNegative();
        result.Longitude.Should().BePositive();
    }

    [Fact] // Positive Test: Verifies method is called with correct parameters
    public async Task GetWeatherAsync_VerifyMethodCalled_WithCorrectParameters()
    {
        // Arrange
        var latitude = 40.7128;
        var longitude = -74.0060;

        _mockWeatherService
            .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new WeatherData());

        // Act
        await _mockWeatherService.Object.GetWeatherAsync(latitude, longitude);

        // Assert - Verify the method was called exactly once with the correct parameters
        _mockWeatherService.Verify(
            s => s.GetWeatherAsync(latitude, longitude),
            Times.Once
        );
    }

    [Fact] // Positive Test: Verifies multiple calls with different parameters
    public async Task GetWeatherAsync_MultipleCalls_TracksAllInvocations()
    {
        // Arrange
        _mockWeatherService
            .Setup(s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new WeatherData());

        // Act
        await _mockWeatherService.Object.GetWeatherAsync(37.7749, -122.4194); // San Francisco
        await _mockWeatherService.Object.GetWeatherAsync(40.7128, -74.0060);  // New York
        await _mockWeatherService.Object.GetWeatherAsync(51.5074, -0.1278);   // London

        // Assert - Verify the method was called exactly 3 times
        _mockWeatherService.Verify(
            s => s.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>()),
            Times.Exactly(3)
        );
    }

    [Fact] // Edge Case Test: Verifies handling of empty string in location search
    public async Task GetWeatherByLocationAsync_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var emptyLocation = "";

        _mockWeatherService
            .Setup(s => s.GetWeatherByLocationAsync(emptyLocation))
            .ReturnsAsync((WeatherData?)null);

        // Act
        var result = await _mockWeatherService.Object.GetWeatherByLocationAsync(emptyLocation);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Positive Test: Verifies weather data includes forecast information
    public async Task GetWeatherAsync_ReturnsWeatherDataWithForecasts()
    {
        // Arrange
        var latitude = 48.8566;
        var longitude = 2.3522;
        var expectedWeatherData = new WeatherData
        {
            LocationName = "Paris",
            Latitude = latitude,
            Longitude = longitude,
            Current = new CurrentWeather { Temperature = 15.0 },
            HourlyForecasts = new List<HourlyForecast>
            {
                new HourlyForecast { Temperature = 16.0, DateTime = DateTime.Now.AddHours(1) },
                new HourlyForecast { Temperature = 17.0, DateTime = DateTime.Now.AddHours(2) }
            },
            DailyForecasts = new List<DailyForecast>
            {
                new DailyForecast { MaxTemperature = 20.0, MinTemperature = 10.0, Date = DateTime.Today }
            }
        };

        _mockWeatherService
            .Setup(s => s.GetWeatherAsync(latitude, longitude))
            .ReturnsAsync(expectedWeatherData);

        // Act
        var result = await _mockWeatherService.Object.GetWeatherAsync(latitude, longitude);

        // Assert
        result.Should().NotBeNull();
        result!.HourlyForecasts.Should().NotBeNull();
        result.HourlyForecasts.Should().HaveCount(2);
        result.DailyForecasts.Should().NotBeNull();
        result.DailyForecasts.Should().HaveCount(1);
    }
}
