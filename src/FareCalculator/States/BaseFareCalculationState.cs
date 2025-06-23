using FareCalculator.Interfaces;
using Microsoft.Extensions.Logging;

namespace FareCalculator.States;

/// <summary>
/// Represents the base fare calculation state that determines the initial fare using available calculation strategies.
/// This state selects the most appropriate strategy based on priority and capability to handle the request.
/// </summary>
public class BaseFareCalculationState : IFareCalculationState
{
    private readonly ILogger<BaseFareCalculationState> _logger;
    private readonly IEnumerable<IFareCalculationStrategy> _strategies;
    private readonly IStationService _stationService;

    /// <summary>
    /// Gets the name of this state in the fare calculation workflow.
    /// </summary>
    /// <value>Returns "BaseFareCalculation" to identify this as the base fare calculation state.</value>
    public string StateName => "BaseFareCalculation";

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseFareCalculationState"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing state execution information.</param>
    /// <param name="strategies">The collection of available fare calculation strategies.</param>
    /// <param name="stationService">The station service for retrieving station information and calculating distances.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public BaseFareCalculationState(
        ILogger<BaseFareCalculationState> logger,
        IEnumerable<IFareCalculationStrategy> strategies,
        IStationService stationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        _stationService = stationService ?? throw new ArgumentNullException(nameof(stationService));
    }

    /// <summary>
    /// Processes the fare calculation context by selecting an appropriate strategy and calculating the base fare.
    /// Also calculates and stores additional journey information such as distance between stations.
    /// </summary>
    /// <param name="context">The fare calculation context containing the request and current state.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated context with base fare.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the context parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no suitable fare calculation strategy is found.</exception>
    public async Task<FareCalculationContext> ProcessAsync(FareCalculationContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _logger.LogInformation("Calculating base fare using available strategies");

        // Select the best strategy based on capability and priority
        var strategy = _strategies
            .Where(s => s.CanHandle(context.Request))
            .OrderByDescending(s => s.Priority)
            .FirstOrDefault();

        if (strategy == null)
        {
            throw new InvalidOperationException("No suitable fare calculation strategy found");
        }

        _logger.LogInformation("Using strategy: {StrategyName}", strategy.StrategyName);
        context.ProcessingLog.Add($"Selected strategy: {strategy.StrategyName}");

        // Calculate base fare using the selected strategy
        context.CurrentFare = await strategy.CalculateBaseFareAsync(context.Request);
        context.Data["BaseFare"] = context.CurrentFare;
        context.Data["StrategyUsed"] = strategy.StrategyName;

        // Calculate additional journey information
        var distance = await _stationService.CalculateDistanceAsync(
            context.Request.Origin, context.Request.Destination);
        context.Response.Distance = Math.Round(distance, 2);
        context.Data["Distance"] = context.Response.Distance;

        // Log the calculation results
        context.ProcessingLog.Add($"Base fare calculated: ${context.CurrentFare:F2}");
        context.ProcessingLog.Add($"Distance: {context.Response.Distance:F2} km");

        _logger.LogInformation("Base fare calculated: ${BaseFare:F2}", context.CurrentFare);

        return context;
    }

    /// <summary>
    /// Determines whether this state can transition to the specified next state.
    /// The base fare calculation state can only transition to the discount application state.
    /// </summary>
    /// <param name="nextState">The proposed next state in the workflow.</param>
    /// <returns>True if the next state is the discount application state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the nextState parameter is null.</exception>
    public bool CanTransitionTo(IFareCalculationState nextState)
    {
        if (nextState == null)
            throw new ArgumentNullException(nameof(nextState));

        return nextState.StateName == "DiscountApplication";
    }
} 