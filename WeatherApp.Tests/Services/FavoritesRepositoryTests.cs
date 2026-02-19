using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Data.Entities;
using WeatherApp.Services;

namespace WeatherApp.Tests.Services;

public class FavoritesRepositoryTests : IDisposable
{
    private readonly WeatherDbContext _context;
    private readonly FavoritesRepository _repository;

    public FavoritesRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WeatherDbContext(options);
        _repository = new FavoritesRepository(_context);
    }

    [Fact] // Positive Test: Verifies favorite can be added to database successfully
    public async Task AddAsync_AddsFavoriteToDatabase()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194,
            LastKnownTemperature = 20.5,
            LastKnownCondition = "Partly cloudy"
        };

        // Act
        await _repository.AddAsync(favorite);
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("San Francisco");
    }

    [Fact] // Positive Test: Verifies retrieval of all favorites sorted by last accessed date
    public async Task GetAllAsync_ReturnsAllFavorites_OrderedByLastAccessed()
    {
        // Arrange
        var favorite1 = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194,
            LastAccessedDate = DateTime.UtcNow.AddHours(-2)
        };
        var favorite2 = new FavoriteLocation
        {
            Name = "New York",
            Country = "USA",
            Latitude = 40.7128,
            Longitude = -74.0060,
            LastAccessedDate = DateTime.UtcNow
        };

        await _repository.AddAsync(favorite1);
        await _repository.AddAsync(favorite2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("New York"); // Most recently accessed
    }

    [Fact] // Positive Test: Verifies retrieval of favorite by valid ID
    public async Task GetByIdAsync_ReturnsFavorite_WhenExists()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194
        };
        await _repository.AddAsync(favorite);

        // Act
        var result = await _repository.GetByIdAsync(favorite.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("San Francisco");
    }

    [Fact] // Negative Test: Verifies null return when ID does not exist
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Positive Test: Verifies coordinate-based search within tolerance range
    public async Task GetByCoordinatesAsync_ReturnsFavorite_WhenWithinTolerance()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194
        };
        await _repository.AddAsync(favorite);

        // Act - Search with slightly different coordinates within 0.01 degree tolerance
        var result = await _repository.GetByCoordinatesAsync(37.7750, -122.4195);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("San Francisco");
    }

    [Fact] // Negative Test: Verifies null return when coordinates outside tolerance
    public async Task GetByCoordinatesAsync_ReturnsNull_WhenOutsideTolerance()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194
        };
        await _repository.AddAsync(favorite);

        // Act - Search with coordinates outside 0.01 degree tolerance
        var result = await _repository.GetByCoordinatesAsync(37.8000, -122.5000);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Positive Test: Verifies favorite properties can be updated
    public async Task UpdateAsync_UpdatesFavorite()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194,
            LastKnownTemperature = 20.5
        };
        await _repository.AddAsync(favorite);

        // Act
        favorite.LastKnownTemperature = 25.0;
        favorite.LastKnownCondition = "Sunny";
        await _repository.UpdateAsync(favorite);

        var result = await _repository.GetByIdAsync(favorite.Id);

        // Assert
        result.Should().NotBeNull();
        result!.LastKnownTemperature.Should().Be(25.0);
        result.LastKnownCondition.Should().Be("Sunny");
    }

    [Fact] // Positive Test: Verifies favorite can be deleted from database
    public async Task DeleteAsync_RemovesFavorite()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194
        };
        await _repository.AddAsync(favorite);

        // Act
        await _repository.DeleteAsync(favorite.Id);
        var result = await _repository.GetByIdAsync(favorite.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Positive Test: Verifies access tracking increments count and updates timestamp
    public async Task RecordAccessAsync_IncrementsAccessCount_AndUpdatesDate()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "San Francisco",
            Country = "USA",
            Latitude = 37.7749,
            Longitude = -122.4194,
            AccessCount = 0,
            LastAccessedDate = null
        };
        await _repository.AddAsync(favorite);

        // Act
        await _repository.RecordAccessAsync(favorite.Id);
        var result = await _repository.GetByIdAsync(favorite.Id);

        // Assert
        result.Should().NotBeNull();
        result!.AccessCount.Should().Be(1);
        result.LastAccessedDate.Should().NotBeNull();
    }

    [Fact] // Negative Test: Verifies graceful handling of access recording for non-existent ID
    public async Task RecordAccessAsync_DoesNothing_WhenIdNotExists()
    {
        // Act - Try to record access for non-existent ID
        await _repository.RecordAccessAsync(999);
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Negative Test: Verifies graceful handling of deleting non-existent favorite
    public async Task DeleteAsync_DoesNothing_WhenIdNotExists()
    {
        // Act - Try to delete non-existent favorite
        await _repository.DeleteAsync(999);
        var allFavorites = await _repository.GetAllAsync();

        // Assert - No exceptions, database unchanged
        allFavorites.Should().BeEmpty();
    }

    [Fact] // Negative Test: Verifies empty list returned when no favorites exist
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoFavorites()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact] // Negative Test: Verifies nullable fields can store null values correctly
    public async Task AddAsync_WithNullValues_StoresNullsCorrectly()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "Unknown Location",
            Region = null, // Nullable field
            Country = "USA",
            Latitude = 0.0,
            Longitude = 0.0,
            LastKnownTemperature = null,
            LastKnownCondition = null
        };

        // Act
        await _repository.AddAsync(favorite);
        var result = await _repository.GetByIdAsync(favorite.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Region.Should().BeNull();
        result.LastKnownTemperature.Should().BeNull();
        result.LastKnownCondition.Should().BeNull();
    }

    [Fact] // Negative Test: Verifies null return when searching empty database
    public async Task GetByCoordinatesAsync_ReturnsNull_WhenNoFavoritesExist()
    {
        // Act
        var result = await _repository.GetByCoordinatesAsync(37.7749, -122.4194);

        // Assert
        result.Should().BeNull();
    }

    [Fact] // Edge Case Test: Verifies handling of negative coordinates during update
    public async Task UpdateAsync_WithNegativeCoordinates_SavesCorrectly()
    {
        // Arrange
        var favorite = new FavoriteLocation
        {
            Name = "Southern Location",
            Country = "Australia",
            Latitude = -33.8688,
            Longitude = 151.2093
        };
        await _repository.AddAsync(favorite);

        // Act
        favorite.Latitude = -34.0;
        await _repository.UpdateAsync(favorite);
        var result = await _repository.GetByIdAsync(favorite.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(-34.0);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
