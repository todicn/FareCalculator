using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using FareCalculator.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FareCalculator.Tests.Strategies;

public class MetroLineFareStrategyTests
{
    private readonly Mock<ILogger<MetroLineFareStrategy>> _mockLogger;
    private readonly Mock<IMetroLineService> _mockMetroLineService;
    private readonly FareCalculationOptions _fareOptions;
    private readonly MetroLineOptions _metroLineOptions;
    private readonly MetroLineFareStrategy _strategy;

    public MetroLineFareStrategyTests()
    {
        _mockLogger = new Mock<ILogger<MetroLineFareStrategy>>();
        _mockMetroLineService = new Mock<IMetroLineService>();

        _fareOptions = new FareCalculationOptions
        {
            ZoneBasedFares = new Dictionary<int, decimal> { { 1, 2.50m }, { 2, 3.75m } },
            ZoneMapping = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } },
            Priorities = new StrategyPriorityOptions { MetroLineFareStrategy = 110 }
        };

        _metroLineOptions = new MetroLineOptions
        {
            LineFareMultipliers = new Dictionary<string, decimal> { { "RL", 1.2m }, { "BL", 1.0m } },
            TransferPenalties = new Dictionary<string, decimal> { { "RL-BL", 0.25m } }
        };

        var fareOptionsWrapper = Options.Create(_fareOptions);
        var metroLineOptionsWrapper = Options.Create(_metroLineOptions);

        _strategy = new MetroLineFareStrategy(
            _mockLogger.Object,
            _mockMetroLineService.Object,
            fareOptionsWrapper,
            metroLineOptionsWrapper);
    }

    [Fact]
    public void StrategyName_ReturnsCorrectName()
    {
        Assert.Equal("Metro Line-Based Calculation", _strategy.StrategyName);
    }

    [Fact]
    public void Priority_ReturnsConfiguredPriority()
    {
        Assert.Equal(110, _strategy.Priority);
    }

    [Fact]
    public void CanHandle_StationsWithMetroLines_ReturnsTrue()
    {
        var request = new FareRequest
        {
            Origin = new Station { MetroLineIds = new List<int> { 1 } },
            Destination = new Station { MetroLineIds = new List<int> { 2 } }
        };

        var result = _strategy.CanHandle(request);

        Assert.True(result);
    }

    [Fact]
    public void CanHandle_OriginWithoutMetroLines_ReturnsFalse()
    {
        var request = new FareRequest
        {
            Origin = new Station { MetroLineIds = new List<int>() },
            Destination = new Station { MetroLineIds = new List<int> { 2 } }
        };

        var result = _strategy.CanHandle(request);

        Assert.False(result);
    }

    [Fact]
    public async Task CalculateBaseFareAsync_DirectRoute_ReturnsCorrectFare()
    {
        // Arrange
        var origin = new Station { Zone = "A", MetroLineIds = new List<int> { 1 } };
        var destination = new Station { Zone = "B", MetroLineIds = new List<int> { 1 } };
        var request = new FareRequest { Origin = origin, Destination = destination };

        var metroLine = new MetroLine { Id = 1, Code = "RL", FareMultiplier = 1.2m };
        var route = new MetroRoute
        {
            Origin = origin,
            Destination = destination,
            TransferCount = 0,
            Segments = new List<RouteSegment>
            {
                new RouteSegment { MetroLine = metroLine }
            }
        };

        _mockMetroLineService.Setup(s => s.CalculateOptimalRouteAsync(origin, destination))
            .ReturnsAsync(route);

        // Act
        var result = await _strategy.CalculateBaseFareAsync(request);

        // Assert
        // Zone fare (A to B = 1 zone difference) * line multiplier = 2.50 * 1.2 = 3.00
        Assert.Equal(3.00m, result);
    }
} 