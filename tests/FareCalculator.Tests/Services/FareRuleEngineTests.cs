using FareCalculator.Configuration;
using FareCalculator.Models;
using FareCalculator.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FareCalculator.Tests.Services;

public class FareRuleEngineTests
{
    private readonly Mock<ILogger<FareRuleEngine>> _mockLogger;
    private readonly FareRuleEngine _fareRuleEngine;

    public FareRuleEngineTests()
    {
        _mockLogger = new Mock<ILogger<FareRuleEngine>>();

        // Create test configuration
        var fareOptions = new FareCalculationOptions
        {
            PassengerDiscounts = new Dictionary<string, decimal>
            {
                { "Adult", 0.00m },
                { "Child", 0.50m },
                { "Senior", 0.30m },
                { "Student", 0.20m },
                { "Disabled", 0.50m }
            },
            TimeBasedRules = new TimeBasedRulesOptions
            {
                PeakHours = new PeakHourOptions
                {
                    Surcharge = 0.25m,
                    WeekdayMorningStart = 7,
                    WeekdayMorningEnd = 9,
                    WeekdayEveningStart = 17,
                    WeekdayEveningEnd = 19
                },
                OffPeakHours = new OffPeakHourOptions
                {
                    Discount = 0.10m,
                    NightStart = 22,
                    NightEnd = 6
                }
            },
            ZoneMapping = new Dictionary<string, int>
            {
                { "A", 1 },
                { "B", 2 },
                { "C", 3 }
            }
        };

        var mockOptions = new Mock<IOptions<FareCalculationOptions>>();
        mockOptions.Setup(x => x.Value).Returns(fareOptions);

        _fareRuleEngine = new FareRuleEngine(_mockLogger.Object, mockOptions.Object);
    }

    [Theory]
    [InlineData(PassengerType.Adult, 10.00, 10.00)]
    [InlineData(PassengerType.Child, 10.00, 5.00)]
    [InlineData(PassengerType.Senior, 10.00, 7.00)]
    [InlineData(PassengerType.Student, 10.00, 8.00)]
    [InlineData(PassengerType.Disabled, 10.00, 5.00)]
    public void ApplyDiscounts_ReturnsCorrectDiscountedFare(PassengerType passengerType, decimal baseFare, decimal expectedFare)
    {
        // Act
        var result = _fareRuleEngine.ApplyDiscounts(baseFare, passengerType);

        // Assert
        Assert.Equal(expectedFare, result);
    }

    [Theory]
    [InlineData(2024, 1, 15, 8, 0, 0, 12.5)] // Monday 8 AM - Peak hour (25% surcharge)
    [InlineData(2024, 1, 15, 18, 0, 0, 12.5)] // Monday 6 PM - Peak hour (25% surcharge)
    [InlineData(2024, 1, 15, 23, 0, 0, 9.0)] // Monday 11 PM - Off-peak (10% discount)
    [InlineData(2024, 1, 15, 3, 0, 0, 9.0)] // Monday 3 AM - Off-peak (10% discount)
    [InlineData(2024, 1, 15, 12, 0, 0, 10.0)] // Monday 12 PM - Regular hours
    [InlineData(2024, 1, 13, 8, 0, 0, 10.0)] // Saturday 8 AM - Regular hours (not weekday)
    public void ApplyTimeBasedRules_ReturnsCorrectFare(int year, int month, int day, int hour, int minute, int second, decimal expectedFare)
    {
        // Arrange
        var baseFare = 10.00m;
        var travelTime = new DateTime(year, month, day, hour, minute, second);

        // Act
        var result = _fareRuleEngine.ApplyTimeBasedRules(baseFare, travelTime);

        // Assert
        Assert.Equal(expectedFare, result);
    }

    [Theory]
    [InlineData("A", "A", 1)] // Same zone
    [InlineData("A", "B", 2)] // Adjacent zones
    [InlineData("A", "C", 3)] // Two zones apart
    [InlineData("B", "A", 2)] // Reverse direction
    [InlineData("C", "A", 3)] // Reverse direction, two zones apart
    public void CalculateNumberOfZones_ReturnsCorrectZoneCount(string originZone, string destinationZone, int expectedZones)
    {
        // Arrange
        var origin = new Station { Zone = originZone };
        var destination = new Station { Zone = destinationZone };

        // Act
        var result = _fareRuleEngine.CalculateNumberOfZones(origin, destination);

        // Assert
        Assert.Equal(expectedZones, result);
    }

    [Fact]
    public void CalculateNumberOfZones_UnknownZone_DefaultsToZone1()
    {
        // Arrange
        var origin = new Station { Zone = "X" }; // Unknown zone
        var destination = new Station { Zone = "A" };

        // Act
        var result = _fareRuleEngine.CalculateNumberOfZones(origin, destination);

        // Assert
        Assert.Equal(1, result); // Should default unknown zone to 1, so 1 to 1 = 1 zone
    }
} 