# Metro Fare Calculator

A C# console application for calculating metro/subway fare costs with support for different passenger types, time-based pricing, and zone-based fare calculation.

## Features

- **Zone-based Fare Calculation**: Calculates fares based on the number of zones traveled
- **Passenger Type Discounts**: Different pricing for adults, children, seniors, students, and disabled passengers
- **Time-based Pricing**: Peak hour surcharges and off-peak discounts
- **Distance Calculation**: Uses Haversine formula to calculate distances between stations
- **Configuration-based System**: All constants (fares, discounts, stations) stored in `appsettings.json`
- **Dependency Injection**: Modern .NET architecture with proper DI container setup
- **Comprehensive Logging**: Structured logging throughout the application
- **Unit & Integration Tests**: Full test coverage with mocking and real dependency testing
- **Complete Documentation**: XML documentation and DocFX-generated API documentation

## Documentation

This project includes comprehensive documentation generated with DocFX:

### 📖 **API Documentation**
- **XML Documentation**: All public APIs are documented with XML comments
- **Generated Documentation Site**: Professional documentation website with DocFX
- **Design Patterns Guide**: Detailed explanation of Strategy and State pattern implementations
- **API Reference**: Complete interface and model documentation

### 🛠️ **Building Documentation**

#### Prerequisites
Install DocFX globally:
```bash
dotnet tool install -g docfx
```

#### Generate Documentation
```bash
# Quick build and serve
docfx docfx.json --serve

# Or use the provided script
powershell -ExecutionPolicy Bypass -File build-docs.ps1

# Manual process
dotnet build                    # Generate XML documentation
docfx build docfx.json         # Build documentation site
docfx serve _site              # Serve locally at http://localhost:8080
```

#### Documentation Structure
- **Main Site**: Generated in `_site/` directory
- **API Reference**: Auto-generated from XML documentation
- **Articles**: Markdown files in `docs/` directory
- **Configuration**: `docfx.json` with proper paths configured

## Architecture

The application follows clean architecture principles with:

- **Models**: Core data structures (`Station`, `FareRequest`, `FareResponse`, `PassengerType`)
- **Interfaces**: Abstractions for all services (`IStationService`, `IFareCalculationService`, `IFareRuleEngine`)
- **Services**: Concrete implementations with business logic
- **Configuration**: Strongly-typed configuration classes for all settings
- **Dependency Injection**: Proper service registration and lifetime management

## Configuration

All hardcoded constants have been moved to `appsettings.json` for easy customization:

### **Fare Settings** (`FareCalculation` section)
```json
{
  "FareCalculation": {
    "Currency": "USD",
    "ZoneBasedFares": {
      "1": 2.50,
      "2": 3.75,
      "3": 5.00
    },
    "DistanceBasedFares": {
      "BaseFare": 1.50,
      "PerKilometerRate": 0.25
    },
    "PassengerDiscounts": {
      "Adult": 0.00,
      "Child": 0.50,
      "Senior": 0.30,
      "Student": 0.20,
      "Disabled": 0.50
    },
    "TimeBasedRules": {
      "PeakHours": {
        "Surcharge": 0.25,
        "WeekdayMorningStart": 7,
        "WeekdayMorningEnd": 9,
        "WeekdayEveningStart": 17,
        "WeekdayEveningEnd": 19
      },
      "OffPeakHours": {
        "Discount": 0.10,
        "NightStart": 22,
        "NightEnd": 6
      }
    },
    "ZoneMapping": {
      "A": 1,
      "B": 2,
      "C": 3
    },
    "Priorities": {
      "ZoneBasedFareStrategy": 100,
      "DistanceBasedFareStrategy": 50,
      "PassengerDiscountStrategy": 100,
      "TimeBasedDiscountStrategy": 90
    }
  }
}
```

### **Station Data** (`Stations` section)
Station coordinates, zones, and names are configurable:
```json
{
  "Stations": [
    {
      "Id": 1,
      "Name": "Downtown Central",
      "Zone": "A",
      "Latitude": 40.7128,
      "Longitude": -74.0060
    }
  ]
}
```

### **Geography Settings** (`Geography` section)
```json
{
  "Geography": {
    "EarthRadiusKilometers": 6371
  }
}
```

### **Configuration Benefits**
- **Easy Customization**: Modify fares without recompiling
- **Environment-specific Settings**: Different configs for dev/prod
- **Dynamic Pricing**: Change time-based rules seasonally
- **Station Management**: Add/modify stations via configuration
- **Strongly-typed**: Compile-time safety with configuration classes

## Fare Rules

### Base Fares by Zone
- Same zone (1 zone): $2.50
- Cross one zone (2 zones): $3.75
- Cross two zones (3 zones): $5.00

### Passenger Discounts
- **Adult**: No discount (0%)
- **Child**: 50% discount
- **Senior**: 30% discount
- **Student**: 20% discount
- **Disabled**: 50% discount

### Time-based Pricing
- **Peak Hours** (weekdays 7-9 AM and 5-7 PM): 25% surcharge
- **Off-Peak Hours** (10 PM - 6 AM): 10% discount
- **Regular Hours**: No adjustment

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- IDE (Visual Studio, VS Code, or JetBrains Rider)

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd FareCalculator
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

### Running the Application

```bash
dotnet run --project src/FareCalculator
```

Follow the interactive prompts to:
1. Select origin and destination stations
2. Choose passenger type
3. Enter travel date/time (or use current time)
4. View calculated fare

### Sample Stations

The application includes 8 predefined stations:

**Zone A:**
- Downtown Central
- Uptown North
- Harbor View

**Zone B:**
- Eastside Plaza
- Westwood Terminal
- University Campus

**Zone C:**
- Southgate Junction
- Airport Express

## Running Tests

### Unit Tests
```bash
dotnet test tests/FareCalculator.Tests --filter Category!=Integration
```

### Integration Tests
```bash
dotnet test tests/FareCalculator.Tests --filter Category=Integration
```

### All Tests
```bash
dotnet test
```

### Test Coverage
The test suite includes:
- **Unit Tests**: Mock-based testing of individual services
- **Integration Tests**: End-to-end testing with real dependencies
- **Theory Tests**: Parameterized tests for multiple scenarios
- **Exception Handling**: Error condition testing

## Project Structure

```
FareCalculator/
├── src/
│   └── FareCalculator/
│       ├── Configuration/       # Strongly-typed configuration classes
│       ├── Interfaces/          # Service abstractions
│       ├── Models/              # Data models
│       ├── Services/            # Business logic implementations
│       ├── States/              # State pattern implementations
│       ├── Strategies/          # Strategy pattern implementations
│       ├── Program.cs           # Application entry point
│       ├── appsettings.json     # Configuration file
│       └── FareCalculator.csproj
├── tests/
│   └── FareCalculator.Tests/
│       ├── Services/            # Unit tests
│       ├── Strategies/          # Strategy tests
│       ├── Integration/         # Integration tests
│       └── FareCalculator.Tests.csproj
├── docs/                        # Documentation files
├── _site/                       # Generated documentation site
├── docfx.json                   # DocFX configuration
├── FareCalculator.sln
└── README.md
```

## Dependencies

### Main Application
- Microsoft.Extensions.Hosting (8.0.0)
- Microsoft.Extensions.DependencyInjection (8.0.0)
- Microsoft.Extensions.Logging (8.0.0)
- Microsoft.Extensions.Logging.Console (8.0.0)
- Microsoft.Extensions.Configuration (8.0.0)
- Microsoft.Extensions.Options.ConfigurationExtensions (8.0.0)

### Testing
- Microsoft.NET.Test.Sdk (17.8.0)
- xunit (2.6.1)
- xunit.runner.visualstudio (2.5.1)
- Moq (4.20.69)

## Example Usage

```
=== Metro Fare Calculator ===

Available Stations:
==================
1. Downtown Central (Zone A)
2. Uptown North (Zone A)
3. Eastside Plaza (Zone B)
4. Westwood Terminal (Zone B)
5. Southgate Junction (Zone C)
6. Airport Express (Zone C)
7. University Campus (Zone B)
8. Harbor View (Zone A)

--- New Fare Calculation ---
Enter origin station name or ID: 1
Enter destination station name or ID: Airport Express

Select passenger type:
1. Adult
2. Child
3. Senior
4. Student
5. Disabled
Enter choice (1-5): 2

Enter travel date and time (YYYY-MM-DD HH:MM) or press Enter for now: 2024-01-15 08:30

=== Fare Calculation Result ===
Fare Amount: $3.13 USD
Fare Type: Child - Peak Hours
Number of Zones: 3
Distance: 32.18 km
Description: Journey from Downtown Central to Airport Express covers 3 zone(s). Base fare: $5.00, Final fare after adjustments: $3.13
===============================
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is open source and available under the [MIT License](LICENSE). 
# Test change to trigger rebuild
