using FareCalculator.Configuration;
using FareCalculator.Models;
using FareCalculator.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FareCalculator.Tests.Services;

public class StationServiceTests
{
    private readonly Mock<ILogger<StationService>> _mockLogger;
    private readonly StationService _stationService;

    public StationServiceTests()
    {
        _mockLogger = new Mock<ILogger<StationService>>();

        // Create test configuration
        var testStations = CreateTestStations();
        var mockStationOptions = new Mock<IOptions<List<Station>>>();
        mockStationOptions.Setup(x => x.Value).Returns(testStations);

        var geographyOptions = new GeographyOptions { EarthRadiusKilometers = 6371.0 };
        var mockGeographyOptions = new Mock<IOptions<GeographyOptions>>();
        mockGeographyOptions.Setup(x => x.Value).Returns(geographyOptions);

        _stationService = new StationService(_mockLogger.Object, mockStationOptions.Object, mockGeographyOptions.Object);
    }

    private static List<Station> CreateTestStations()
    {
        return new List<Station>
        {
            new() { Id = 1, Name = "Downtown Central", Zone = "A", Latitude = 40.7128, Longitude = -74.0060 },
            new() { Id = 2, Name = "Uptown North", Zone = "A", Latitude = 40.7831, Longitude = -73.9712 },
            new() { Id = 3, Name = "Eastside Plaza", Zone = "B", Latitude = 40.7505, Longitude = -73.9934 },
            new() { Id = 4, Name = "Westwood Terminal", Zone = "B", Latitude = 40.7282, Longitude = -74.0776 },
            new() { Id = 5, Name = "Southgate Junction", Zone = "C", Latitude = 40.6892, Longitude = -74.0445 },
            new() { Id = 6, Name = "Airport Express", Zone = "C", Latitude = 40.6413, Longitude = -73.7781 },
            new() { Id = 7, Name = "University Campus", Zone = "B", Latitude = 40.8176, Longitude = -73.9782 },
            new() { Id = 8, Name = "Harbor View", Zone = "A", Latitude = 40.7061, Longitude = -74.0087 }
        };
    }

    [Fact]
    public async Task GetStationByIdAsync_ValidId_ReturnsStation()
    {
        // Act
        var result = await _stationService.GetStationByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Downtown Central", result.Name);
        Assert.Equal("A", result.Zone);
    }

    [Fact]
    public async Task GetStationByIdAsync_InvalidId_ReturnsNull()
    {
        // Act
        var result = await _stationService.GetStationByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStationByNameAsync_ValidName_ReturnsStation()
    {
        // Act
        var result = await _stationService.GetStationByNameAsync("Downtown Central");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Downtown Central", result.Name);
    }

    [Fact]
    public async Task GetStationByNameAsync_CaseInsensitive_ReturnsStation()
    {
        // Act
        var result = await _stationService.GetStationByNameAsync("downtown central");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Downtown Central", result.Name);
    }

    [Fact]
    public async Task GetStationByNameAsync_InvalidName_ReturnsNull()
    {
        // Act
        var result = await _stationService.GetStationByNameAsync("Nonexistent Station");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllStationsAsync_ReturnsAllStations()
    {
        // Act
        var result = await _stationService.GetAllStationsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.Count());
        Assert.Contains(result, s => s.Name == "Downtown Central");
        Assert.Contains(result, s => s.Name == "Airport Express");
    }

    [Fact]
    public async Task CalculateDistanceAsync_ValidStations_ReturnsDistance()
    {
        // Arrange
        var origin = new Station { Latitude = 40.7128, Longitude = -74.0060 }; // Downtown Central
        var destination = new Station { Latitude = 40.7831, Longitude = -73.9712 }; // Uptown North

        // Act
        var result = await _stationService.CalculateDistanceAsync(origin, destination);

        // Assert
        Assert.True(result > 0);
        Assert.True(result < 20); // Reasonable distance for NYC metro stations
    }

    [Fact]
    public async Task CalculateDistanceAsync_SameStation_ReturnsZero()
    {
        // Arrange
        var station = new Station { Latitude = 40.7128, Longitude = -74.0060 };

        // Act
        var result = await _stationService.CalculateDistanceAsync(station, station);

        // Assert
        Assert.Equal(0, result, 1); // Allow for small floating point errors
    }
} 