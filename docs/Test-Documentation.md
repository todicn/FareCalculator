# Test Documentation

## Overview

The FareCalculator project includes comprehensive test coverage with **54 test cases** across unit tests, integration tests, and strategy/state pattern tests. All tests use xUnit framework with Moq for mocking dependencies.

## Test Statistics
- **Total Tests**: 54
- **Unit Tests**: 45  
- **Integration Tests**: 9
- **Success Rate**: 100% (54/54 passing)
- **Test Coverage**: Comprehensive coverage of all major components

## Test Suites

### 1. StationServiceTests (8 tests)
**Location**: `tests/FareCalculator.Tests/Services/StationServiceTests.cs`

**Test Methods:**
- `GetStationByIdAsync_ValidId_ReturnsStation`
- `GetStationByIdAsync_InvalidId_ReturnsNull`
- `GetStationByNameAsync_ValidName_ReturnsStation`
- `GetStationByNameAsync_CaseInsensitive_ReturnsStation`
- `GetStationByNameAsync_InvalidName_ReturnsNull`
- `GetAllStationsAsync_ReturnsAllStations`
- `CalculateDistanceAsync_ValidStations_ReturnsDistance`
- `CalculateDistanceAsync_SameStation_ReturnsZero`

### 2. FareRuleEngineTests (12 tests)
**Location**: `tests/FareCalculator.Tests/Services/FareRuleEngineTests.cs`

**Passenger Discount Tests:**
- Adult: 0% discount
- Child: 50% discount  
- Senior: 30% discount
- Student: 20% discount
- Disabled: 50% discount

**Time-Based Pricing Tests:**
- Peak hours (weekday 7-9 AM, 5-7 PM): +25% surcharge
- Off-peak hours (10 PM - 6 AM): -10% discount
- Regular hours: No adjustment
- Weekend exclusions

### 3. ZoneBasedFareStrategyTests (8 tests)
**Location**: `tests/FareCalculator.Tests/Strategies/ZoneBasedFareStrategyTests.cs`

**Zone Fare Test Cases:**
- 1 zone: $2.50
- 2 zones: $3.75
- 3+ zones: $5.00

### 4. PassengerDiscountStrategyTests (8 tests)
**Location**: `tests/FareCalculator.Tests/Strategies/PassengerDiscountStrategyTests.cs`

Tests all passenger type discounts and strategy behavior.

### 5. Integration Tests (9 tests)
**Location**: `tests/FareCalculator.Tests/Integration/FareCalculationIntegrationTests.cs`

**End-to-End Scenarios:**
- Same zone adult fare: $2.50
- Cross zone child fare with discount
- Peak hour surcharges
- Off-peak discounts
- Combined discount scenarios

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~StationServiceTests"
```

## Test Infrastructure

- **Configuration Mocking**: All tests use mock configuration
- **Dependency Injection**: Full DI container setup in integration tests
- **Mock Framework**: Moq for unit test isolation
- **Theory Tests**: Parameterized tests for multiple scenarios

## Test Execution

### **Running Tests**

```