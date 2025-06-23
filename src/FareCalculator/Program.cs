using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using FareCalculator.Services;
using FareCalculator.Strategies;
using FareCalculator.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FareCalculator;

class Program
{
    static async Task Main(string[] args)
    {
        // Check if running in visualization mode
        if (args.Length > 0 && args[0].ToLower() == "--visualize")
        {
            await Visualization.VisualizationDemo.RunAsync();
            return;
        }

        // Create host with dependency injection
        var host = CreateHostBuilder(args).Build();
        
        // Get services
        var fareCalculationService = host.Services.GetRequiredService<IFareCalculationService>();
        var stationService = host.Services.GetRequiredService<IStationService>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Metro Fare Calculator started");

        try
        {
            await RunCalculatorAsync(fareCalculationService, stationService, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while running the fare calculator");
        }
        finally
        {
            logger.LogInformation("Metro Fare Calculator stopped");
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure strongly-typed configuration options
                services.Configure<FareCalculationOptions>(
                    context.Configuration.GetSection(FareCalculationOptions.SectionName));
                services.Configure<GeographyOptions>(
                    context.Configuration.GetSection(GeographyOptions.SectionName));
                services.Configure<List<Station>>(
                    context.Configuration.GetSection(StationOptions.SectionName));
                services.Configure<MetroLineOptions>(
                    context.Configuration.GetSection(MetroLineOptions.SectionName));

                // Register core services
                services.AddScoped<IStationService, StationService>();
                services.AddScoped<IMetroLineService, MetroLineService>();
                services.AddScoped<IFareRuleEngine, FareRuleEngine>();
                
                // Register strategy pattern implementations
                services.AddScoped<IFareCalculationStrategy, Strategies.ZoneBasedFareStrategy>();
                services.AddScoped<IFareCalculationStrategy, Strategies.DistanceBasedFareStrategy>();
                services.AddScoped<IFareCalculationStrategy, Strategies.MetroLineFareStrategy>();
                services.AddScoped<IDiscountStrategy, Strategies.PassengerDiscountStrategy>();
                services.AddScoped<IDiscountStrategy, Strategies.TimeBasedDiscountStrategy>();
                
                // Register state pattern implementations
                services.AddScoped<States.InitialCalculationState>();
                services.AddScoped<States.BaseFareCalculationState>();
                services.AddScoped<States.DiscountApplicationState>();
                services.AddScoped<States.FinalCalculationState>();
                
                // Register state machine
                services.AddScoped<IFareCalculationStateMachine, FareCalculationStateMachine>();
                
                // Register fare calculation service
                services.AddScoped<IFareCalculationService, FareCalculationService>();
                
                // Register visualization services
                services.AddScoped<Visualization.MetroMapGenerator>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });
    }

    static async Task RunCalculatorAsync(
        IFareCalculationService fareCalculationService, 
        IStationService stationService, 
        ILogger<Program> logger)
    {
        Console.WriteLine("=== Metro Fare Calculator ===");
        Console.WriteLine();

        // Display available stations
        await DisplayAvailableStationsAsync(stationService);

        while (true)
        {
            try
            {
                Console.WriteLine("\n--- New Fare Calculation ---");
                
                // Get origin station
                var originStation = await GetStationFromUserAsync(stationService, "Enter origin station name or ID: ");
                if (originStation == null)
                {
                    Console.WriteLine("Invalid origin station. Please try again.");
                    continue;
                }

                // Get destination station
                var destinationStation = await GetStationFromUserAsync(stationService, "Enter destination station name or ID: ");
                if (destinationStation == null)
                {
                    Console.WriteLine("Invalid destination station. Please try again.");
                    continue;
                }

                if (originStation.Id == destinationStation.Id)
                {
                    Console.WriteLine("Origin and destination cannot be the same. Please try again.");
                    continue;
                }

                // Get passenger type
                var passengerType = GetPassengerTypeFromUser();

                // Get travel date/time
                var travelDate = GetTravelDateFromUser();

                // Create fare request
                var fareRequest = new FareRequest
                {
                    Origin = originStation,
                    Destination = destinationStation,
                    PassengerType = passengerType,
                    TravelDate = travelDate
                };

                // Calculate fare
                var fareResponse = await fareCalculationService.CalculateFareAsync(fareRequest);

                // Display result
                DisplayFareResult(fareResponse);

                // Ask if user wants to calculate another fare
                Console.WriteLine("\nWould you like to calculate another fare? (y/n): ");
                var continueChoice = Console.ReadLine()?.ToLower();
                if (continueChoice != "y" && continueChoice != "yes")
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during fare calculation");
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Please try again.");
            }
        }

        Console.WriteLine("\nThank you for using Metro Fare Calculator!");
    }

    static async Task DisplayAvailableStationsAsync(IStationService stationService)
    {
        Console.WriteLine("Available Stations:");
        Console.WriteLine("==================");
        var stations = await stationService.GetAllStationsAsync();
        
        foreach (var station in stations.OrderBy(s => s.Id))
        {
            Console.WriteLine($"{station.Id}. {station.Name} (Zone {station.Zone})");
        }
        Console.WriteLine();
    }

    static async Task<Station?> GetStationFromUserAsync(IStationService stationService, string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(input))
            return null;

        // Try to parse as ID first
        if (int.TryParse(input, out int stationId))
        {
            return await stationService.GetStationByIdAsync(stationId);
        }

        // Try to find by name
        return await stationService.GetStationByNameAsync(input);
    }

    static PassengerType GetPassengerTypeFromUser()
    {
        Console.WriteLine("\nSelect passenger type:");
        Console.WriteLine("1. Adult");
        Console.WriteLine("2. Child");
        Console.WriteLine("3. Senior");
        Console.WriteLine("4. Student");
        Console.WriteLine("5. Disabled");
        Console.Write("Enter choice (1-5): ");

        var choice = Console.ReadLine()?.Trim();
        return choice switch
        {
            "1" => PassengerType.Adult,
            "2" => PassengerType.Child,
            "3" => PassengerType.Senior,
            "4" => PassengerType.Student,
            "5" => PassengerType.Disabled,
            _ => PassengerType.Adult
        };
    }

    static DateTime GetTravelDateFromUser()
    {
        Console.Write("\nEnter travel date and time (YYYY-MM-DD HH:MM) or press Enter for now: ");
        var input = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(input))
            return DateTime.Now;

        if (DateTime.TryParse(input, out DateTime travelDate))
            return travelDate;

        Console.WriteLine("Invalid date format. Using current time.");
        return DateTime.Now;
    }

    static void DisplayFareResult(FareResponse fareResponse)
    {
        Console.WriteLine("\n=== Fare Calculation Result ===");
        Console.WriteLine($"Fare Amount: ${fareResponse.Amount:F2} {fareResponse.Currency}");
        Console.WriteLine($"Fare Type: {fareResponse.FareType}");
        Console.WriteLine($"Number of Zones: {fareResponse.NumberOfZones}");
        Console.WriteLine($"Distance: {fareResponse.Distance:F2} km");
        Console.WriteLine($"Description: {fareResponse.Description}");
        Console.WriteLine("===============================");
    }
} 