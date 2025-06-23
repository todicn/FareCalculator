# API Documentation Guide

## Overview

The FareCalculator project generates comprehensive XML documentation that can be used by IDEs, documentation generators, and API tools to provide IntelliSense, parameter hints, and detailed API documentation.

## XML Documentation Files

### 📁 **Generated Files**
- **Main Project**: `src/FareCalculator/bin/Debug/net8.0/FareCalculator.xml` (52KB)
- **Test Project**: `tests/FareCalculator.Tests/bin/Debug/net8.0/FareCalculator.Tests.xml`

### 🔧 **Usage in IDEs**
The XML documentation files are automatically used by:
- **Visual Studio**: Provides IntelliSense with parameter descriptions and method summaries
- **VS Code**: Shows hover information and parameter hints
- **JetBrains Rider**: Enhanced code completion and documentation tooltips

## API Reference

### 🏛️ **Core Interfaces**

#### `IFareCalculationService`
Main service interface for fare calculations.
```csharp
Task<FareResponse> CalculateFareAsync(FareRequest request);
```

#### `IStationService`
Station management and distance calculation services.
```csharp
Task<Station?> GetStationByIdAsync(int id);
Task<Station?> GetStationByNameAsync(string name);
Task<IEnumerable<Station>> GetAllStationsAsync();
Task<double> CalculateDistanceAsync(Station origin, Station destination);
```

#### `IFareCalculationStrategy`
Strategy pattern interface for fare calculation algorithms.
```csharp
string StrategyName { get; }
Task<decimal> CalculateBaseFareAsync(FareRequest request);
bool CanHandle(FareRequest request);
int Priority { get; }
```

#### `IDiscountStrategy`
Strategy pattern interface for discount applications.
```csharp
string StrategyName { get; }
decimal ApplyDiscount(decimal baseFare, FareRequest request);
bool AppliesTo(PassengerType passengerType);
int Priority { get; }
```

#### `IFareCalculationState`
State pattern interface for workflow management.
```csharp
string StateName { get; }
Task<FareCalculationContext> ProcessAsync(FareCalculationContext context);
bool CanTransitionTo(IFareCalculationState nextState);
```

### 📊 **Data Models**

#### `Station`
Represents a metro station with location and zone information.
```csharp
public class Station
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Zone { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
```

#### `FareRequest`
Input data for fare calculation requests.
```csharp
public class FareRequest
{
    public Station Origin { get; set; }
    public Station Destination { get; set; }
    public PassengerType PassengerType { get; set; }
    public DateTime TravelDate { get; set; }
}
```

#### `FareResponse`
Output data containing calculated fare information.
```csharp
public class FareResponse
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public int NumberOfZones { get; set; }
    public double Distance { get; set; }
    public string FareType { get; set; }
    public string Description { get; set; }
}
```

#### `PassengerType`
Enum defining passenger classifications for discount eligibility.
```csharp
public enum PassengerType
{
    Adult,      // No discount
    Child,      // 50% discount
    Senior,     // 30% discount
    Student,    // 20% discount
    Disabled    // 50% discount
}
```

### 🔄 **Workflow States**

#### `InitialCalculationState`
- Validates request data
- Initializes calculation context
- Sets up logging and response object

#### `BaseFareCalculationState`
- Selects appropriate fare calculation strategy
- Calculates base fare amount
- Determines distance and zone information

#### `DiscountApplicationState`
- Applies all applicable discount strategies
- Tracks discount amounts and audit trail
- Processes discounts in priority order

#### `FinalCalculationState`
- Rounds final fare amount
- Calculates zone count
- Generates comprehensive fare description

### 🎯 **Strategy Implementations**

#### Fare Calculation Strategies
1. **ZoneBasedFareStrategy** (Priority: 100)
   - Primary strategy for zone-based calculations
   - Uses zone mapping: 1 zone = $2.50, 2 zones = $3.75, 3+ zones = $5.00

2. **DistanceBasedFareStrategy** (Priority: 50)
   - Fallback strategy using GPS coordinates
   - Rate: $0.25 per kilometer

#### Discount Strategies
1. **PassengerDiscountStrategy** (Priority: 100)
   - Applies passenger type discounts
   - Child/Disabled: 50%, Senior: 30%, Student: 20%, Adult: 0%

2. **TimeBasedDiscountStrategy** (Priority: 90)
   - Peak hours (7-9 AM, 5-7 PM weekdays): +25% surcharge
   - Off-peak hours (10 PM - 6 AM): -10% discount

## Documentation Generation Commands

### 🔨 **Build Commands**
```bash
# Build with XML documentation
dotnet build

# Build specific configuration
dotnet build --configuration Release

# Verbose build output
dotnet build --verbosity detailed
```

### 📖 **Documentation Tools**
The generated XML files can be used with:
- **DocFX**: Microsoft's documentation generation tool
- **Sandcastle**: Windows-based documentation generator
- **Swagger/OpenAPI**: For API documentation if exposing as web API
- **NDoc**: Legacy documentation generator

## Example Usage

### Basic Fare Calculation
```csharp
// Create request
var request = new FareRequest
{
    Origin = new Station { Id = 1, Name = "Downtown Central", Zone = "A" },
    Destination = new Station { Id = 6, Name = "Airport Express", Zone = "C" },
    PassengerType = PassengerType.Adult,
    TravelDate = DateTime.Now
};

// Calculate fare
var response = await fareCalculationService.CalculateFareAsync(request);

// Result: $5.00 for 3-zone journey
```

### With Discounts
```csharp
var request = new FareRequest
{
    Origin = new Station { Id = 1, Name = "Downtown Central", Zone = "A" },
    Destination = new Station { Id = 6, Name = "Airport Express", Zone = "C" },
    PassengerType = PassengerType.Child,  // 50% discount
    TravelDate = new DateTime(2024, 1, 1, 3, 0, 0)  // Off-peak: -10%
};

var response = await fareCalculationService.CalculateFareAsync(request);

// Result: $2.25 (Base: $5.00 → Child discount: $2.50 → Off-peak: $2.25)
```

## Testing Documentation

The test project also generates XML documentation covering:
- **Unit Tests**: Individual component testing
- **Integration Tests**: End-to-end workflow testing
- **Strategy Tests**: Pattern implementation testing
- **State Tests**: Workflow state testing

### Test Coverage
- **54 Total Tests**: Comprehensive coverage of all functionality
- **Pattern Testing**: Both Strategy and State patterns fully tested  
- **Error Handling**: Exception scenarios and edge cases covered
- **Business Logic**: All fare rules and discount calculations verified 