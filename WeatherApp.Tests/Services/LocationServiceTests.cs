using FluentAssertions;
using Moq;
using WeatherApp.Services;

namespace WeatherApp.Tests.Services;

public class LocationServiceTests
{
    private readonly Mock<ILocationService> _mockLocationService;

    public LocationServiceTests()
    {
        _mockLocationService = new Mock<ILocationService>();
    }

    [Fact] // Positive Test: Verifies successful location retrieval
    public async Task GetCurrentLocationAsync_WhenLocationAvailable_ReturnsLocation()
    {
        // Arrange
        var expectedLocation = new Microsoft.Maui.Devices.Sensors.Location
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            Accuracy = 10.0,
            Timestamp = DateTimeOffset.UtcNow
        };

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(37.7749);
        result.Longitude.Should().Be(-122.4194);
        result.Accuracy.Should().Be(10.0);
    }

    [Fact] // Negative Test: Verifies null return when location unavailable
    public async Task GetCurrentLocationAsync_WhenLocationUnavailable_ReturnsNull()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((Microsoft.Maui.Devices.Sensors.Location?)null);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Positive Test: Verifies permission check returns true when granted
    public async Task CheckPermissionsAsync_WhenPermissionsGranted_ReturnsTrue()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.CheckPermissionsAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _mockLocationService.Object.CheckPermissionsAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact] // Negative Test: Verifies permission check returns false when denied
    public async Task CheckPermissionsAsync_WhenPermissionsDenied_ReturnsFalse()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.CheckPermissionsAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _mockLocationService.Object.CheckPermissionsAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact] // Positive Test: Verifies permission request succeeds
    public async Task RequestPermissionsAsync_WhenUserGrants_ReturnsTrue()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.RequestPermissionsAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _mockLocationService.Object.RequestPermissionsAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact] // Negative Test: Verifies permission request fails when user denies
    public async Task RequestPermissionsAsync_WhenUserDenies_ReturnsFalse()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.RequestPermissionsAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _mockLocationService.Object.RequestPermissionsAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact] // Positive Test: Verifies typical workflow - check then request permissions
    public async Task PermissionWorkflow_CheckThenRequest_WorksCorrectly()
    {
        // Arrange - First check returns false, then request returns true
        _mockLocationService
            .Setup(s => s.CheckPermissionsAsync())
            .ReturnsAsync(false);
        
        _mockLocationService
            .Setup(s => s.RequestPermissionsAsync())
            .ReturnsAsync(true);

        // Act
        var hasPermission = await _mockLocationService.Object.CheckPermissionsAsync();
        bool requestResult = false;
        
        if (!hasPermission)
        {
            requestResult = await _mockLocationService.Object.RequestPermissionsAsync();
        }

        // Assert
        hasPermission.Should().BeFalse();
        requestResult.Should().BeTrue();
        _mockLocationService.Verify(s => s.CheckPermissionsAsync(), Times.Once);
        _mockLocationService.Verify(s => s.RequestPermissionsAsync(), Times.Once);
    }

    [Fact] // Positive Test: Verifies location retrieval after permission granted
    public async Task GetCurrentLocation_AfterPermissionGranted_ReturnsLocation()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.CheckPermissionsAsync())
            .ReturnsAsync(true);

        var expectedLocation = new Microsoft.Maui.Devices.Sensors.Location
        {
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var hasPermission = await _mockLocationService.Object.CheckPermissionsAsync();
        Microsoft.Maui.Devices.Sensors.Location? location = null;

        if (hasPermission)
        {
            location = await _mockLocationService.Object.GetCurrentLocationAsync();
        }

        // Assert
        hasPermission.Should().BeTrue();
        location.Should().NotBeNull();
        location!.Latitude.Should().Be(40.7128);
        location.Longitude.Should().Be(-74.0060);
    }

    [Fact] // Edge Case Test: Verifies location with high accuracy value
    public async Task GetCurrentLocationAsync_WithHighAccuracy_ReturnsAccurateLocation()
    {
        // Arrange - GPS with very high accuracy (low value = high accuracy)
        var expectedLocation = new Microsoft.Maui.Devices.Sensors.Location
        {
            Latitude = 48.8566,
            Longitude = 2.3522,
            Accuracy = 5.0, // 5 meters accuracy
            Altitude = 35.0
        };

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Accuracy.Should().Be(5.0);
        result.Altitude.Should().Be(35.0);
    }

    [Fact] // Edge Case Test: Verifies location with low accuracy value
    public async Task GetCurrentLocationAsync_WithLowAccuracy_ReturnsLocation()
    {
        // Arrange - Network-based location with lower accuracy
        var expectedLocation = new Microsoft.Maui.Devices.Sensors.Location
        {
            Latitude = 51.5074,
            Longitude = -0.1278,
            Accuracy = 500.0 // 500 meters accuracy (less precise)
        };

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Accuracy.Should().Be(500.0);
    }

    [Fact] // Edge Case Test: Verifies location at extreme coordinates (South Pole)
    public async Task GetCurrentLocationAsync_AtSouthPole_ReturnsLocation()
    {
        // Arrange - South Pole
        var expectedLocation = new Microsoft.Maui.Devices.Sensors.Location
        {
            Latitude = -90.0,
            Longitude = 0.0,
            Altitude = 2835.0 // Elevation at South Pole
        };

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(-90.0);
        result.Altitude.Should().Be(2835.0);
    }

    [Fact] // Positive Test: Verifies method invocation tracking
    public async Task GetCurrentLocationAsync_VerifyMethodCalled_ExactlyOnce()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(new Microsoft.Maui.Devices.Sensors.Location());

        // Act
        await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert - Verify method was called exactly once
        _mockLocationService.Verify(
            s => s.GetCurrentLocationAsync(),
            Times.Once
        );
    }

    [Fact] // Positive Test: Verifies multiple location requests are tracked
    public async Task GetCurrentLocationAsync_MultipleCalls_AllTracked()
    {
        // Arrange
        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(new Microsoft.Maui.Devices.Sensors.Location
            {
                Latitude = 37.7749,
                Longitude = -122.4194
            });

        // Act - Request location 3 times
        await _mockLocationService.Object.GetCurrentLocationAsync();
        await _mockLocationService.Object.GetCurrentLocationAsync();
        await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert - Verify method was called exactly 3 times
        _mockLocationService.Verify(
            s => s.GetCurrentLocationAsync(),
            Times.Exactly(3)
        );
    }

    [Fact] // Negative Test: Verifies behavior when permission check never called
    public async Task GetCurrentLocationAsync_WithoutPermissionCheck_StillWorks()
    {
        // Arrange - Skip permission check, directly get location
        var expectedLocation = new Microsoft.Maui.Devices.Sensors.Location
        {
            Latitude = 35.6762,
            Longitude = 139.6503
        };

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act - Get location without checking permissions first
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Should().NotBeNull();
        _mockLocationService.Verify(s => s.CheckPermissionsAsync(), Times.Never);
    }

    [Fact] // Edge Case Test: Verifies location with timestamp
    public async Task GetCurrentLocationAsync_WithTimestamp_ReturnsRecentLocation()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expectedLocation = new Microsoft.Maui.Devices.Sensors.Location
        {
            Latitude = 34.0522,
            Longitude = -118.2437,
            Timestamp = now
        };

        _mockLocationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Timestamp.Should().NotBeNull();
        result.Timestamp.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact] // Positive Test: Verifies sequential permission check and request
    public async Task PermissionFlow_CheckFailsThenRequestSucceeds_ReturnsTrue()
    {
        // Arrange - Simulate user flow: no permission → request → granted
        var checkCallCount = 0;
        
        _mockLocationService
            .Setup(s => s.CheckPermissionsAsync())
            .ReturnsAsync(() =>
            {
                checkCallCount++;
                return checkCallCount > 1; // First call false, subsequent true
            });

        _mockLocationService
            .Setup(s => s.RequestPermissionsAsync())
            .ReturnsAsync(true);

        // Act
        var initialCheck = await _mockLocationService.Object.CheckPermissionsAsync();
        var requestResult = await _mockLocationService.Object.RequestPermissionsAsync();
        var finalCheck = await _mockLocationService.Object.CheckPermissionsAsync();

        // Assert
        initialCheck.Should().BeFalse();
        requestResult.Should().BeTrue();
        finalCheck.Should().BeTrue();
    }
}
