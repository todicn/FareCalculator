# API Reference

Welcome to the Metro Fare Calculator API documentation. This section contains the complete API reference for all public classes, interfaces, and methods.

## Core Namespaces

### FareCalculator.Configuration
Configuration classes for strongly-typed settings management.

**Key Classes:**
- `FareCalculationOptions` - Main fare calculation settings
- `GeographyOptions` - Geographical calculation constants
- `StationOptions` - Station data configuration

### FareCalculator.Interfaces
Service contracts and abstractions.

**Key Interfaces:**
- `IFareCalculationService` - Main fare calculation service
- `IStationService` - Station management operations
- `IFareRuleEngine` - Business rule processing
- `IFareCalculationStrategy` - Strategy pattern for fare calculation
- `IDiscountStrategy` - Strategy pattern for discount application
- `IFareCalculationState` - State pattern for workflow management

### FareCalculator.Models
Core data structures and entities.

**Key Classes:**
- `Station` - Metro station with location and zone information
- `FareRequest` - Fare calculation input parameters
- `FareResponse` - Fare calculation results
- `PassengerType` - Passenger classification enumeration

### FareCalculator.Services
Business logic implementations.

**Key Classes:**
- `FareCalculationService` - Main fare calculation implementation
- `StationService` - Station management and distance calculations
- `FareRuleEngine` - Business rules and fare adjustments
- `FareCalculationStateMachine` - State machine orchestration

### FareCalculator.Strategies
Strategy pattern implementations for fare calculation algorithms.

**Key Classes:**
- `ZoneBasedFareStrategy` - Zone-based fare calculation
- `DistanceBasedFareStrategy` - Distance-based fare calculation  
- `PassengerDiscountStrategy` - Passenger type discounts
- `TimeBasedDiscountStrategy` - Time-based fare adjustments

### FareCalculator.States
State pattern implementations for fare calculation workflow.

**Key Classes:**
- `InitialCalculationState` - Request validation and setup
- `BaseFareCalculationState` - Base fare calculation
- `DiscountApplicationState` - Discount strategy application
- `FinalCalculationState` - Final fare calculation and formatting

## Quick Start

### Basic Fare Calculation
```csharp
// Inject the service
IFareCalculationService fareService;

// Create a fare request
var request = new FareRequest
{
    Origin = new Station { Id = 1, Name = "Downtown Central", Zone = "A" },
    Destination = new Station { Id = 6, Name = "Airport Express", Zone = "C" },
    PassengerType = PassengerType.Adult,
    TravelDate = DateTime.Now
};

// Calculate fare
var response = await fareService.CalculateFareAsync(request);
Console.WriteLine($"Fare: ${response.Amount:F2}");
```

### Station Management
```csharp
// Inject the service
IStationService stationService;

// Get station by ID
var station = await stationService.GetStationByIdAsync(1);

// Get all stations
var allStations = await stationService.GetAllStationsAsync();

// Calculate distance between stations
var distance = await stationService.CalculateDistanceAsync(origin, destination);
```

## Architecture Patterns

The API implements several design patterns:

- **Strategy Pattern**: Pluggable fare calculation algorithms
- **State Pattern**: Workflow-based fare processing
- **Dependency Injection**: Loose coupling and testability
- **Options Pattern**: Strongly-typed configuration
- **Repository Pattern**: Data access abstraction

## Configuration

All services use the .NET Options pattern for configuration:

```csharp
// Register configuration
services.Configure<FareCalculationOptions>(
    configuration.GetSection("FareCalculation"));

// Inject into services
public class FareCalculationService
{
    public FareCalculationService(IOptions<FareCalculationOptions> options)
    {
        _options = options.Value;
    }
}
```

See the [Configuration Documentation](../articles/Configuration-Migration.md) for complete details.

## Error Handling

All services include comprehensive error handling:

- `ArgumentNullException` - For null parameters
- `ArgumentException` - For invalid parameter values
- `InvalidOperationException` - For invalid state transitions

## Thread Safety

All services are designed to be thread-safe and can be registered as singletons or scoped services in the DI container.

## Performance

The API is optimized for performance:

- Efficient distance calculations using Haversine formula
- Caching-friendly design
- Minimal memory allocations
- Fast lookup operations

## Testing

The API is fully covered by unit and integration tests. See [Test Documentation](../articles/Test-Documentation.md) for details. 