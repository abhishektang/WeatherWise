using FluentAssertions;
using WeatherApp.Data.Entities;

namespace WeatherApp.Tests.Entities;

public class FavoriteLocationTests
{
    [Fact] // Positive Test: Verifies basic FavoriteLocation object creation with valid data
    public void FavoriteLocation_CanBeCreated()
    {
        // Arrange & Act
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Region = "California",
            Country = "United States",
            Latitude = 37.7749,
            Longitude = -122.4194,
            LastKnownTemperature = 20.5,
            LastKnownCondition = "Partly cloudy",
            AccessCount = 0
        };

        // Assert
        favorite.Name.Should().Be("San Francisco");
        favorite.Country.Should().Be("United States");
        favorite.Latitude.Should().Be(37.7749);
        favorite.LastKnownTemperature.Should().Be(20.5);
        favorite.AccessCount.Should().Be(0);
    }

    [Fact] // Positive Test: Verifies AddedDate automatically set to current time
    public void FavoriteLocation_AddedDate_DefaultsToUtcNow()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow.AddSeconds(-1);
        
        // Act
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Latitude = 37.7749,
            Longitude = -122.4194
        };
        
        var afterCreate = DateTime.UtcNow.AddSeconds(1);

        // Assert
        favorite.AddedDate.Should().BeAfter(beforeCreate);
        favorite.AddedDate.Should().BeBefore(afterCreate);
    }

    [Fact] // Positive Test: Verifies AccessCount defaults to 0 on creation
    public void FavoriteLocation_AccessCount_DefaultsToZero()
    {
        // Act
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Latitude = 37.7749,
            Longitude = -122.4194
        };

        // Assert
        favorite.AccessCount.Should().Be(0);
    }

    [Fact] // Negative Test: Verifies LastAccessedDate can be null for new favorites
    public void FavoriteLocation_LastAccessedDate_CanBeNull()
    {
        // Act
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Latitude = 37.7749,
            Longitude = -122.4194
        };

        // Assert
        favorite.LastAccessedDate.Should().BeNull();
    }

    [Fact] // Edge Case Test: Verifies handling of negative latitude (southern hemisphere)
    public void FavoriteLocation_WithNegativeCoordinates_StoresCorrectly()
    {
        // Arrange & Act - Southern hemisphere location
        var favorite = new FavoriteLocation
        {
            Name = "Cape Town",
            Country = "South Africa",
            Latitude = -33.9249,
            Longitude = 18.4241
        };

        // Assert
        favorite.Latitude.Should().BeNegative();
        favorite.Longitude.Should().BePositive();
    }

    [Fact] // Negative Test: Verifies handling of null optional fields
    public void FavoriteLocation_WithNullOptionalFields_CreatesSuccessfully()
    {
        // Act
        var favorite = new FavoriteLocation
        {
            Name = "Unknown Location",
            Country = "USA",
            Latitude = 0.0,
            Longitude = 0.0,
            Region = null,
            LastKnownTemperature = null,
            LastKnownCondition = null
        };

        // Assert
        favorite.Name.Should().Be("Unknown Location");
        favorite.Region.Should().BeNull();
        favorite.LastKnownTemperature.Should().BeNull();
        favorite.LastKnownCondition.Should().BeNull();
    }

    [Fact] // Edge Case Test: Verifies handling of zero coordinates (Null Island)
    public void FavoriteLocation_WithZeroCoordinates_StoresCorrectly()
    {
        // Arrange & Act - Null Island (0,0)
        var favorite = new FavoriteLocation
        {
            Name = "Null Island",
            Country = "None",
            Latitude = 0.0,
            Longitude = 0.0
        };

        // Assert
        favorite.Latitude.Should().Be(0.0);
        favorite.Longitude.Should().Be(0.0);
    }

    [Fact] // Positive Test: Verifies AccessCount can be incremented correctly
    public void FavoriteLocation_AccessCount_CanBeIncremented()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "Test Location",
            Latitude = 10.0,
            Longitude = 20.0,
            AccessCount = 0
        };

        // Act
        favorite.AccessCount++;
        favorite.AccessCount++;

        // Assert
        favorite.AccessCount.Should().Be(2);
    }

    [Fact] // Edge Case Test: Verifies handling of negative temperature values
    public void FavoriteLocation_WithNegativeTemperature_StoresCorrectly()
    {
        // Arrange & Act - Arctic location
        var favorite = new FavoriteLocation
        {
            Name = "Nuuk",
            Country = "Greenland",
            Latitude = 64.1814,
            Longitude = -51.6941,
            LastKnownTemperature = -25.5,
            LastKnownCondition = "Snow"
        };

        // Assert
        favorite.LastKnownTemperature.Should().Be(-25.5);
        favorite.LastKnownCondition.Should().Be("Snow");
    }
}
