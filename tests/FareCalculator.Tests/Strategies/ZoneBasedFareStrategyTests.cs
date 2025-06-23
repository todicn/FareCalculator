using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using FareCalculator.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FareCalculator.Tests.Strategies;

public class ZoneBasedFareStrategyTests
{
    private readonly Mock<ILogger<ZoneBasedFareStrategy>> _mockLogger;
    private readonly Mock<IFareRuleEngine> _mockFareRuleEngine;
    private readonly ZoneBasedFareStrategy _strategy;

    public ZoneBasedFareStrategyTests()
    {
        _mockLogger = new Mock<ILogger<ZoneBasedFareStrategy>>();
        _mockFareRuleEngine = new Mock<IFareRuleEngine>();

        // Create test configuration
        var fareOptions = new FareCalculationOptions
        {
            ZoneBasedFares = new Dictionary<int, decimal>
            {
                { 1, 2.50m },
                { 2, 3.75m },
                { 3, 5.00m }
            },
            Priorities = new StrategyPriorityOptions
            {
                ZoneBasedFareStrategy = 100
            }
        };

        var mockOptions = new Mock<IOptions<FareCalculationOptions>>();
        mockOptions.Setup(x => x.Value).Returns(fareOptions);

        _strategy = new ZoneBasedFareStrategy(_mockLogger.Object, _mockFareRuleEngine.Object, mockOptions.Object);
    }

    [Theory]
    [InlineData(1, 2.50)]
    [InlineData(2, 3.75)]
    [InlineData(3, 5.00)]
    [InlineData(4, 5.00)] // Default for unknown zones
    public async Task CalculateBaseFareAsync_DifferentZones_ReturnsCorrectFare(int zones, decimal expectedFare)
    {
        // Arrange
        var request = new FareRequest
        {
            Origin = new Station { Zone = "A" },
            Destination = new Station { Zone = "B" }
        };
        
        _mockFareRuleEngine.Setup(x => x.CalculateNumberOfZones(request.Origin, request.Destination))
            .Returns(zones);

        // Act
        var result = await _strategy.CalculateBaseFareAsync(request);

        // Assert
        Assert.Equal(expectedFare, result);
    }

    [Fact]
    public void CanHandle_ValidZones_ReturnsTrue()
    {
        // Arrange
        var request = new FareRequest
        {
            Origin = new Station { Zone = "A" },
            Destination = new Station { Zone = "B" }
        };

        // Act
        var result = _strategy.CanHandle(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_EmptyZones_ReturnsFalse()
    {
        // Arrange
        var request = new FareRequest
        {
            Origin = new Station { Zone = "" },
            Destination = new Station { Zone = "B" }
        };

        // Act
        var result = _strategy.CanHandle(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Priority_ReturnsExpectedValue()
    {
        // Act & Assert
        Assert.Equal(100, _strategy.Priority);
    }

    [Fact]
    public void StrategyName_ReturnsExpectedValue()
    {
        // Act & Assert
        Assert.Equal("Zone-Based Calculation", _strategy.StrategyName);
    }
} 