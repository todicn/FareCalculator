using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FareCalculator.Strategies;

/// <summary>
/// Fare calculation strategy that considers metro line characteristics including line multipliers and transfer penalties.
/// </summary>
public class MetroLineFareStrategy : IFareCalculationStrategy
{
    private readonly ILogger<MetroLineFareStrategy> _logger;
    private readonly IMetroLineService _metroLineService;
    private readonly FareCalculationOptions _fareOptions;
    private readonly MetroLineOptions _metroLineOptions;

    /// <summary>
    /// Gets the strategy name for identification purposes.
    /// </summary>
    public string StrategyName => "Metro Line-Based Calculation";

    /// <summary>
    /// Gets the priority of this strategy relative to other fare calculation strategies.
    /// </summary>
    public int Priority => _fareOptions.Priorities.MetroLineFareStrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetroLineFareStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing strategy execution information.</param>
    /// <param name="metroLineService">The metro line service for route calculations.</param>
    /// <param name="fareOptions">Configuration options for fare calculation.</param>
    /// <param name="metroLineOptions">Configuration options for metro line settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public MetroLineFareStrategy(
        ILogger<MetroLineFareStrategy> logger,
        IMetroLineService metroLineService,
        IOptions<FareCalculationOptions> fareOptions,
        IOptions<MetroLineOptions> metroLineOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metroLineService = metroLineService ?? throw new ArgumentNullException(nameof(metroLineService));
        _fareOptions = fareOptions?.Value ?? throw new ArgumentNullException(nameof(fareOptions));
        _metroLineOptions = metroLineOptions?.Value ?? throw new ArgumentNullException(nameof(metroLineOptions));
    }

    /// <summary>
    /// Calculates the base fare considering metro line characteristics and route complexity.
    /// </summary>
    /// <param name="request">The fare calculation request containing trip details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the calculated base fare.</returns>
    public async Task<decimal> CalculateBaseFareAsync(FareRequest request)
    {
        _logger.LogInformation("Calculating metro line-based fare for {Origin} to {Destination}", 
            request.Origin.Name, request.Destination.Name);

        // Calculate the optimal route to determine line usage and transfers
        var route = await _metroLineService.CalculateOptimalRouteAsync(request.Origin, request.Destination);
        
        decimal totalFare = 0m;

        // Calculate base fare for each segment
        foreach (var segment in route.Segments)
        {
            var segmentFare = CalculateSegmentFare(segment, request);
            totalFare += segmentFare;

            _logger.LogInformation("Segment fare on {LineCode}: ${SegmentFare}", 
                segment.MetroLine.Code, segmentFare);
        }

        // Apply transfer penalties
        if (route.TransferCount > 0)
        {
            var transferPenalty = CalculateTransferPenalty(route);
            totalFare += transferPenalty;

            _logger.LogInformation("Transfer penalty for {TransferCount} transfers: ${TransferPenalty}", 
                route.TransferCount, transferPenalty);
        }

        _logger.LogInformation("Total metro line-based fare: ${TotalFare}", totalFare);
        return totalFare;
    }

    /// <summary>
    /// Determines if this strategy can handle the given fare request.
    /// </summary>
    /// <param name="request">The fare calculation request to evaluate.</param>
    /// <returns>True if the strategy can handle the request; otherwise, false.</returns>
    public bool CanHandle(FareRequest request)
    {
        // Can handle any request where both stations have metro line associations
        return request.Origin.MetroLineIds.Any() && request.Destination.MetroLineIds.Any();
    }

    /// <summary>
    /// Calculates the fare for a specific route segment.
    /// </summary>
    /// <param name="segment">The route segment to calculate fare for.</param>
    /// <param name="request">The original fare request for context.</param>
    /// <returns>The calculated fare for the segment.</returns>
    private decimal CalculateSegmentFare(RouteSegment segment, FareRequest request)
    {
        // Start with zone-based fare as base
        var zoneFare = _fareOptions.GetZoneBasedFare(
            Math.Abs(_fareOptions.GetZoneValue(request.Origin.Zone) - 
                    _fareOptions.GetZoneValue(request.Destination.Zone)));

        // Apply metro line multiplier
        var lineMultiplier = segment.MetroLine.FareMultiplier;
        var configMultiplier = _metroLineOptions.GetLineFareMultiplier(segment.MetroLine.Code);
        
        // Use the higher of the two multipliers
        var effectiveMultiplier = Math.Max(lineMultiplier, configMultiplier);

        return zoneFare * effectiveMultiplier;
    }

    /// <summary>
    /// Calculates transfer penalties for a route with multiple segments.
    /// </summary>
    /// <param name="route">The route with transfer information.</param>
    /// <returns>The total transfer penalty amount.</returns>
    private decimal CalculateTransferPenalty(MetroRoute route)
    {
        decimal totalPenalty = 0m;

        // For each transfer, apply a penalty based on the lines being transferred between
        for (int i = 0; i < route.Segments.Count - 1; i++)
        {
            var fromLine = route.Segments[i].MetroLine;
            var toLine = route.Segments[i + 1].MetroLine;
            
            var penalty = _metroLineOptions.GetTransferPenalty(fromLine.Code, toLine.Code);
            if (penalty == 0m)
            {
                // Default transfer penalty if not configured
                penalty = 0.50m;
            }
            
            totalPenalty += penalty;
        }

        return totalPenalty;
    }
} 