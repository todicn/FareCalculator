using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FareCalculator.Strategies;

public class ZoneBasedFareStrategy : IFareCalculationStrategy
{
    private readonly ILogger<ZoneBasedFareStrategy> _logger;
    private readonly IFareRuleEngine _fareRuleEngine;
    private readonly FareCalculationOptions _options;

    public string StrategyName => "Zone-Based Calculation";
    public int Priority => _options.Priorities.ZoneBasedFareStrategy;

    public ZoneBasedFareStrategy(
        ILogger<ZoneBasedFareStrategy> logger, 
        IFareRuleEngine fareRuleEngine,
        IOptions<FareCalculationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fareRuleEngine = fareRuleEngine ?? throw new ArgumentNullException(nameof(fareRuleEngine));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<decimal> CalculateBaseFareAsync(FareRequest request)
    {
        _logger.LogInformation("Calculating zone-based fare for {Origin} to {Destination}", 
            request.Origin.Name, request.Destination.Name);

        var numberOfZones = _fareRuleEngine.CalculateNumberOfZones(request.Origin, request.Destination);
        var baseFare = _options.GetZoneBasedFare(numberOfZones);

        _logger.LogInformation("Zone-based calculation: {NumberOfZones} zones = ${BaseFare}", 
            numberOfZones, baseFare);

        return Task.FromResult(baseFare);
    }

    public bool CanHandle(FareRequest request)
    {
        // Can handle any request with valid zones
        return !string.IsNullOrEmpty(request.Origin.Zone) && !string.IsNullOrEmpty(request.Destination.Zone);
    }
} 