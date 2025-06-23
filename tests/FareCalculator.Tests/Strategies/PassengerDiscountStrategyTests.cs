using FareCalculator.Configuration;
using FareCalculator.Models;
using FareCalculator.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FareCalculator.Tests.Strategies;

public class PassengerDiscountStrategyTests
{
    private readonly Mock<ILogger<PassengerDiscountStrategy>> _mockLogger;
    private readonly PassengerDiscountStrategy _strategy;

    public PassengerDiscountStrategyTests()
    {
        _mockLogger = new Mock<ILogger<PassengerDiscountStrategy>>();

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
            Priorities = new StrategyPriorityOptions
            {
                PassengerDiscountStrategy = 100
            }
        };

        var mockOptions = new Mock<IOptions<FareCalculationOptions>>();
        mockOptions.Setup(x => x.Value).Returns(fareOptions);

        _strategy = new PassengerDiscountStrategy(_mockLogger.Object, mockOptions.Object);
    }

    [Theory]
    [InlineData(PassengerType.Adult, 10.00, 10.00)]
    [InlineData(PassengerType.Child, 10.00, 5.00)]
    [InlineData(PassengerType.Senior, 10.00, 7.00)]
    [InlineData(PassengerType.Student, 10.00, 8.00)]
    [InlineData(PassengerType.Disabled, 10.00, 5.00)]
    public void ApplyDiscount_DifferentPassengerTypes_ReturnsCorrectDiscount(
        PassengerType passengerType, decimal baseFare, decimal expectedFare)
    {
        // Arrange
        var request = new FareRequest { PassengerType = passengerType };

        // Act
        var result = _strategy.ApplyDiscount(baseFare, request);

        // Assert
        Assert.Equal(expectedFare, result);
    }

    [Theory]
    [InlineData(PassengerType.Adult)]
    [InlineData(PassengerType.Child)]
    [InlineData(PassengerType.Senior)]
    [InlineData(PassengerType.Student)]
    [InlineData(PassengerType.Disabled)]
    public void AppliesTo_AllPassengerTypes_ReturnsTrue(PassengerType passengerType)
    {
        // Act
        var result = _strategy.AppliesTo(passengerType);

        // Assert
        Assert.True(result);
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
        Assert.Equal("Passenger Type Discount", _strategy.StrategyName);
    }
} 