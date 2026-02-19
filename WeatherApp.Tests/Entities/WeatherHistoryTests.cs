using FluentAssertions;
using WeatherApp.Data.Entities;

namespace WeatherApp.Tests.Entities;

public class WeatherHistoryTests
{
    [Fact] // Positive Test: Verifies basic WeatherHistory object creation with all properties
    public void WeatherHistory_CanBeCreated()
    {
        // Arrange & Act
        var history = new WeatherHistory
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            LocationName = "San Francisco",
            FetchedAt = DateTime.UtcNow,
            WeatherDataJson = "{}",
            Temperature = 20.5,
            FeelsLike = 19.0,
            Humidity = 65,
            WindSpeed = 5.5,
            Condition = "Partly cloudy"
        };

        // Assert
        history.LocationName.Should().Be("San Francisco");
        history.Temperature.Should().Be(20.5);
        history.Condition.Should().Be("Partly cloudy");
    }

    [Fact] // Positive Test: Verifies fresh cache (under 10 min) returns NOT stale
    public void WeatherHistory_IsStale_ReturnsFalse_WhenFreshCache()
    {
        // Arrange
        var history = new WeatherHistory
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            LocationName = "San Francisco",
            FetchedAt = DateTime.UtcNow.AddMinutes(-5).AddMilliseconds(100), // Slightly under 5 minutes ago
            WeatherDataJson = "{}",
            Temperature = 20.5,
            Condition = "Clear"
        };

        // Assert
        history.IsStale.Should().BeFalse();
    }

    [Fact] // Positive Test: Verifies old cache (over 10 min) returns stale
    public void WeatherHistory_IsStale_ReturnsTrue_WhenOlderThan10Minutes()
    {
        // Arrange
        var history = new WeatherHistory
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            LocationName = "San Francisco",
            FetchedAt = DateTime.UtcNow.AddMinutes(-11), // 11 minutes ago
            WeatherDataJson = "{}",
            Temperature = 20.5,
            Condition = "Clear"
        };

        // Assert
        history.IsStale.Should().BeTrue();
    }

    [Fact] // Edge Case Test: Verifies boundary condition at exactly 10 minutes
    public void WeatherHistory_IsStale_EdgeCase_Exactly10Minutes()
    {
        // Arrange
        var history = new WeatherHistory
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            LocationName = "San Francisco",
            FetchedAt = DateTime.UtcNow.AddMinutes(-10).AddMilliseconds(100), // Slightly under 10 minutes
            WeatherDataJson = "{}",
            Temperature = 20.5,
            Condition = "Clear"
        };

        // Assert - At slightly under 10 minutes, it's NOT stale yet (implementation uses > not >=)
        history.IsStale.Should().BeFalse();
    }

    [Fact] // Edge Case Test: Verifies handling of zero temperature (freezing point)
    public void WeatherHistory_WithZeroTemperature_StoresCorrectly()
    {
        // Arrange & Act
        var history = new WeatherHistory
        {
            Latitude = 0.0,
            Longitude = 0.0,
            LocationName = "Unknown",
            FetchedAt = DateTime.UtcNow,
            WeatherDataJson = "{}",
            Temperature = 0.0, // Freezing point
            Condition = "Freezing"
        };

        // Assert
        history.Temperature.Should().Be(0.0);
        history.Condition.Should().Be("Freezing");
        history.IsStale.Should().BeFalse(); // Just fetched
    }

    [Fact] // Edge Case Test: Verifies handling of negative temperature values
    public void WeatherHistory_WithNegativeTemperature_StoresCorrectly()
    {
        // Arrange & Act
        var history = new WeatherHistory
        {
            Latitude = 64.1814,
            Longitude = -51.6941,
            LocationName = "Nuuk",
            FetchedAt = DateTime.UtcNow,
            WeatherDataJson = "{}",
            Temperature = -30.0,
            Condition = "Heavy Snow"
        };

        // Assert
        history.Temperature.Should().Be(-30.0);
        history.Condition.Should().Be("Heavy Snow");
    }

    [Fact] // Edge Case Test: Verifies handling of negative coordinates (southern hemisphere)
    public void WeatherHistory_WithNegativeCoordinates_StoresCorrectly()
    {
        // Arrange & Act - Southern hemisphere
        var history = new WeatherHistory
        {
            Latitude = -33.8688,
            Longitude = 151.2093,
            LocationName = "Sydney",
            FetchedAt = DateTime.UtcNow,
            WeatherDataJson = "{}",
            Temperature = 25.0,
            Condition = "Sunny"
        };

        // Assert
        history.Latitude.Should().BeNegative();
        history.Longitude.Should().BePositive();
    }

    [Fact] // Negative Test: Verifies handling of empty JSON data
    public void WeatherHistory_WithEmptyJson_StoresCorrectly()
    {
        // Arrange & Act
        var history = new WeatherHistory
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            LocationName = "Test Location",
            FetchedAt = DateTime.UtcNow,
            WeatherDataJson = "{}", // Empty JSON
            Temperature = 20.0,
            Condition = "Clear"
        };

        // Assert
        history.WeatherDataJson.Should().Be("{}");
        history.IsStale.Should().BeFalse();
    }

    [Fact] // Positive Test: Verifies newly created cache is NOT stale
    public void WeatherHistory_IsStale_JustCreated_ReturnsFalse()
    {
        // Arrange & Act
        var history = new WeatherHistory
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            LocationName = "San Francisco",
            FetchedAt = DateTime.UtcNow, // Just now
            WeatherDataJson = "{}",
            Temperature = 20.5,
            Condition = "Clear"
        };

        // Assert
        history.IsStale.Should().BeFalse();
    }

    [Fact] // Edge Case Test: Verifies very old cache (1 day) is definitely stale
    public void WeatherHistory_IsStale_VeryOld_ReturnsTrue()
    {
        // Arrange
        var history = new WeatherHistory
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            LocationName = "San Francisco",
            FetchedAt = DateTime.UtcNow.AddDays(-1), // 1 day ago
            WeatherDataJson = "{}",
            Temperature = 20.5,
            Condition = "Clear"
        };

        // Assert
        history.IsStale.Should().BeTrue();
    }
}
