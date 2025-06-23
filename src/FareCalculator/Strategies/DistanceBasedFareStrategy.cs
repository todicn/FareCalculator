using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FareCalculator.Strategies;

public class DistanceBasedFareStrategy : IFareCalculationStrategy
{
    private readonly ILogger<DistanceBasedFareStrategy> _logger;
    private readonly IStationService _stationService;
    private readonly FareCalculationOptions _options;

    public string StrategyName => "Distance-Based Calculation";
    public int Priority => _options.Priorities.DistanceBasedFareStrategy;

    public DistanceBasedFareStrategy(
        ILogger<DistanceBasedFareStrategy> logger, 
        IStationService stationService,
        IOptions<FareCalculationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stationService = stationService ?? throw new ArgumentNullException(nameof(stationService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<decimal> CalculateBaseFareAsync(FareRequest request)
    {
        _logger.LogInformation("Calculating distance-based fare for {Origin} to {Destination}", 
            request.Origin.Name, request.Destination.Name);

        var distance = await _stationService.CalculateDistanceAsync(request.Origin, request.Destination);
        var fare = _options.DistanceBasedFares.BaseFare + (decimal)distance * _options.DistanceBasedFares.PerKilometerRate;

        _logger.LogInformation("Distance-based calculation: {Distance:F2} km = ${Fare:F2}", distance, fare);

        return fare;
    }

    public bool CanHandle(FareRequest request)
    {
        // Can handle any request with valid coordinates
        return request.Origin.Latitude != 0 && request.Origin.Longitude != 0 &&
               request.Destination.Latitude != 0 && request.Destination.Longitude != 0;
    }
} 