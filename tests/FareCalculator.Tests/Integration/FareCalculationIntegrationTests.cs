using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using FareCalculator.Services;
using FareCalculator.States;
using FareCalculator.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FareCalculator.Tests.Integration;

public class FareCalculationIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public FareCalculationIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Configure logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Configure options
        var fareOptions = new FareCalculationOptions
        {
            Currency = "USD",
            ZoneBasedFares = new Dictionary<int, decimal> { { 1, 2.50m }, { 2, 3.75m }, { 3, 5.00m } },
            DistanceBasedFares = new DistanceBasedFareOptions { BaseFare = 1.50m, PerKilometerRate = 0.25m },
            PassengerDiscounts = new Dictionary<string, decimal> { { "Adult", 0.00m }, { "Child", 0.50m }, { "Senior", 0.30m }, { "Student", 0.20m }, { "Disabled", 0.50m } },
            TimeBasedRules = new TimeBasedRulesOptions
            {
                PeakHours = new PeakHourOptions { Surcharge = 0.25m, WeekdayMorningStart = 7, WeekdayMorningEnd = 9, WeekdayEveningStart = 17, WeekdayEveningEnd = 19 },
                OffPeakHours = new OffPeakHourOptions { Discount = 0.10m, NightStart = 22, NightEnd = 6 }
            },
            ZoneMapping = new Dictionary<string, int> { { "A", 1 }, { "B", 2 }, { "C", 3 } },
            Priorities = new StrategyPriorityOptions { ZoneBasedFareStrategy = 100, DistanceBasedFareStrategy = 50, PassengerDiscountStrategy = 100, TimeBasedDiscountStrategy = 90 }
        };

        var geographyOptions = new GeographyOptions { EarthRadiusKilometers = 6371.0 };

        var stationsList = new List<Station>
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

        services.Configure<FareCalculationOptions>(options =>
        {
            options.Currency = fareOptions.Currency;
            options.ZoneBasedFares = fareOptions.ZoneBasedFares;
            options.DistanceBasedFares = fareOptions.DistanceBasedFares;
            options.PassengerDiscounts = fareOptions.PassengerDiscounts;
            options.TimeBasedRules = fareOptions.TimeBasedRules;
            options.ZoneMapping = fareOptions.ZoneMapping;
            options.Priorities = fareOptions.Priorities;
        });

        services.Configure<GeographyOptions>(options =>
        {
            options.EarthRadiusKilometers = geographyOptions.EarthRadiusKilometers;
        });

        services.Configure<List<Station>>(options =>
        {
            options.Clear();
            options.AddRange(stationsList);
        });
        
        // Register core services
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<IFareRuleEngine, FareRuleEngine>();
        
        // Register strategy pattern implementations
        services.AddScoped<IFareCalculationStrategy, ZoneBasedFareStrategy>();
        services.AddScoped<IFareCalculationStrategy, DistanceBasedFareStrategy>();
        services.AddScoped<IDiscountStrategy, PassengerDiscountStrategy>();
        services.AddScoped<IDiscountStrategy, TimeBasedDiscountStrategy>();
        
        // Register state pattern implementations
        services.AddScoped<InitialCalculationState>();
        services.AddScoped<BaseFareCalculationState>();
        services.AddScoped<DiscountApplicationState>();
        services.AddScoped<FinalCalculationState>();
        
        // Register state machine
        services.AddScoped<IFareCalculationStateMachine, FareCalculationStateMachine>();
        
        // Register fare calculation service
        services.AddScoped<IFareCalculationService, FareCalculationService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task EndToEndFareCalculation_AdultSameZoneRegularHours_ReturnsCorrectFare()
    {
        // Arrange
        var fareCalculationService = _serviceProvider.GetRequiredService<IFareCalculationService>();
        var stationService = _serviceProvider.GetRequiredService<IStationService>();
        
        var origin = await stationService.GetStationByNameAsync("Downtown Central");
        var destination = await stationService.GetStationByNameAsync("Harbor View");
        
        var request = new FareRequest
        {
            Origin = origin!,
            Destination = destination!,
            PassengerType = PassengerType.Adult,
            TravelDate = new DateTime(2024, 1, 15, 14, 0, 0) // Monday 2 PM
        };

        // Act
        var result = await fareCalculationService.CalculateFareAsync(request);

        // Assert
        Assert.Equal(2.50m, result.Amount); // Same zone fare
        Assert.Equal("USD", result.Currency);
        Assert.Equal(1, result.NumberOfZones);
        Assert.Contains("Adult - Regular Hours", result.FareType);
        Assert.Contains("Zone-Based Calculation", result.Description);
        Assert.True(result.Distance > 0);
    }

    [Fact]
    public async Task EndToEndFareCalculation_ChildCrossZonePeakHours_ReturnsDiscountedFare()
    {
        // Arrange
        var fareCalculationService = _serviceProvider.GetRequiredService<IFareCalculationService>();
        var stationService = _serviceProvider.GetRequiredService<IStationService>();
        
        var origin = await stationService.GetStationByNameAsync("Downtown Central");
        var destination = await stationService.GetStationByNameAsync("Airport Express");
        
        var request = new FareRequest
        {
            Origin = origin!,
            Destination = destination!,
            PassengerType = PassengerType.Child,
            TravelDate = new DateTime(2024, 1, 15, 8, 30, 0) // Monday 8:30 AM (peak hour)
        };

        // Act
        var result = await fareCalculationService.CalculateFareAsync(request);

        // Assert
        // Zone A to Zone C = 3 zones = $5.00 base
        // Child discount: 50% = $2.50
        // Peak hour surcharge: 25% = $3.125 = $3.12 rounded
        Assert.Equal(3.12m, result.Amount);
        Assert.Equal(3, result.NumberOfZones);
        Assert.Contains("Child - Peak Hours", result.FareType);
        Assert.Contains("Zone-Based Calculation", result.Description);
    }

    [Fact]
    public async Task EndToEndFareCalculation_SeniorOffPeakHours_ReturnsDiscountedFare()
    {
        // Arrange
        var fareCalculationService = _serviceProvider.GetRequiredService<IFareCalculationService>();
        var stationService = _serviceProvider.GetRequiredService<IStationService>();
        
        var origin = await stationService.GetStationByNameAsync("Eastside Plaza");
        var destination = await stationService.GetStationByNameAsync("Westwood Terminal");
        
        var request = new FareRequest
        {
            Origin = origin!,
            Destination = destination!,
            PassengerType = PassengerType.Senior,
            TravelDate = new DateTime(2024, 1, 15, 23, 0, 0) // Monday 11 PM (off-peak)
        };

        // Act
        var result = await fareCalculationService.CalculateFareAsync(request);

        // Assert
        // Zone B to Zone B = 1 zone = $2.50 base
        // Senior discount: 30% = $1.75
        // Off-peak discount: 10% = $1.575 = $1.58 rounded
        Assert.Equal(1.58m, result.Amount);
        Assert.Equal(1, result.NumberOfZones);
        Assert.Contains("Senior - Off-Peak Hours", result.FareType);
        Assert.Contains("Zone-Based Calculation", result.Description);
    }

    [Fact]
    public async Task StateMachine_ProcessWithContext_ReturnsDetailedProcessingLog()
    {
        // Arrange
        var stateMachine = _serviceProvider.GetRequiredService<IFareCalculationStateMachine>();
        var stationService = _serviceProvider.GetRequiredService<IStationService>();
        
        var origin = await stationService.GetStationByNameAsync("Eastside Plaza");
        var destination = await stationService.GetStationByNameAsync("Westwood Terminal");
        
        var request = new FareRequest
        {
            Origin = origin!,
            Destination = destination!,
            PassengerType = PassengerType.Senior,
            TravelDate = new DateTime(2024, 1, 15, 23, 0, 0) // Monday 11 PM (off-peak)
        };

        // Act
        var context = await stateMachine.ProcessWithContextAsync(request);

        // Assert
        Assert.NotNull(context.ProcessingLog);
        Assert.True(context.ProcessingLog.Count > 5);
        Assert.Contains(context.ProcessingLog, log => log.Contains("Entering state: Initial"));
        Assert.Contains(context.ProcessingLog, log => log.Contains("Entering state: BaseFareCalculation"));
        Assert.Contains(context.ProcessingLog, log => log.Contains("Entering state: DiscountApplication"));
        Assert.Contains(context.ProcessingLog, log => log.Contains("Entering state: FinalCalculation"));
    }

    [Fact]
    public async Task StrategySelection_ZoneBasedHasPriorityOverDistance_UsesZoneBased()
    {
        // Arrange
        var stateMachine = _serviceProvider.GetRequiredService<IFareCalculationStateMachine>();
        var stationService = _serviceProvider.GetRequiredService<IStationService>();
        
        var origin = await stationService.GetStationByNameAsync("Downtown Central");
        var destination = await stationService.GetStationByNameAsync("Uptown North");
        
        var request = new FareRequest
        {
            Origin = origin!,
            Destination = destination!,
            PassengerType = PassengerType.Adult,
            TravelDate = DateTime.Now
        };

        // Act
        var context = await stateMachine.ProcessWithContextAsync(request);

        // Assert
        Assert.Equal("Zone-Based Calculation", context.Data["StrategyUsed"]);
        Assert.Contains("Zone-Based Calculation", context.Response.Description);
    }

    [Fact]
    public async Task StationService_GetAllStations_ReturnsExpectedStations()
    {
        // Arrange
        var stationService = _serviceProvider.GetRequiredService<IStationService>();

        // Act
        var stations = await stationService.GetAllStationsAsync();

        // Assert
        Assert.Equal(8, stations.Count());
        
        var stationList = stations.ToList();
        Assert.Contains(stationList, s => s.Name == "Downtown Central" && s.Zone == "A");
        Assert.Contains(stationList, s => s.Name == "Airport Express" && s.Zone == "C");
        Assert.Contains(stationList, s => s.Name == "University Campus" && s.Zone == "B");
    }

    [Theory]
    [InlineData("Downtown Central", "Uptown North")] // Zone A to A (same zone)
    [InlineData("Downtown Central", "Eastside Plaza")] // Zone A to B (cross zone)
    [InlineData("Eastside Plaza", "Airport Express")] // Zone B to C (cross zone)
    public async Task EndToEndDistanceCalculation_ValidStations_ReturnsReasonableDistance(string originName, string destinationName)
    {
        // Arrange
        var stationService = _serviceProvider.GetRequiredService<IStationService>();
        
        var origin = await stationService.GetStationByNameAsync(originName);
        var destination = await stationService.GetStationByNameAsync(destinationName);

        // Act
        var distance = await stationService.CalculateDistanceAsync(origin!, destination!);

        // Assert
        Assert.True(distance >= 0);
        Assert.True(distance <= 50); // Reasonable maximum distance for metro system
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
} 