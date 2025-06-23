using FareCalculator.Interfaces;
using Microsoft.Extensions.Logging;

namespace FareCalculator.States;

/// <summary>
/// Represents the initial state in the fare calculation workflow that initializes the calculation context.
/// This state sets up the basic information and prepares the context for subsequent processing states.
/// </summary>
public class InitialCalculationState : IFareCalculationState
{
    private readonly ILogger<InitialCalculationState> _logger;

    /// <summary>
    /// Gets the name of this state in the fare calculation workflow.
    /// </summary>
    /// <value>Returns "Initial" to identify this as the initial calculation state.</value>
    public string StateName => "Initial";

    /// <summary>
    /// Initializes a new instance of the <see cref="InitialCalculationState"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing state execution information.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public InitialCalculationState(ILogger<InitialCalculationState> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the fare calculation context by initializing basic information and logging the journey details.
    /// Sets up the response currency and records the calculation start time and journey parameters.
    /// </summary>
    /// <param name="context">The fare calculation context to initialize.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the initialized context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the context parameter is null.</exception>
    public Task<FareCalculationContext> ProcessAsync(FareCalculationContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _logger.LogInformation("Starting fare calculation for {Origin} to {Destination}",
            context.Request.Origin.Name, context.Request.Destination.Name);

        // Log initial calculation details
        context.ProcessingLog.Add($"Initialized calculation at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        context.ProcessingLog.Add($"Route: {context.Request.Origin.Name} → {context.Request.Destination.Name}");
        context.ProcessingLog.Add($"Passenger: {context.Request.PassengerType}");
        context.ProcessingLog.Add($"Travel Date: {context.Request.TravelDate:yyyy-MM-dd HH:mm}");

        // Initialize response with default values
        context.Response.Currency = "USD";
        
        return Task.FromResult(context);
    }

    /// <summary>
    /// Determines whether this state can transition to the specified next state.
    /// The initial state can only transition to the base fare calculation state.
    /// </summary>
    /// <param name="nextState">The proposed next state in the workflow.</param>
    /// <returns>True if the next state is the base fare calculation state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the nextState parameter is null.</exception>
    public bool CanTransitionTo(IFareCalculationState nextState)
    {
        if (nextState == null)
            throw new ArgumentNullException(nameof(nextState));

        return nextState.StateName == "BaseFareCalculation";
    }
} 