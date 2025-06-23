using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;

namespace FareCalculator.Services;

/// <summary>
/// Defines the contract for a state machine that orchestrates the fare calculation workflow through multiple processing states.
/// </summary>
public interface IFareCalculationStateMachine
{
    /// <summary>
    /// Processes a fare calculation request through all states and returns the final fare response.
    /// </summary>
    /// <param name="request">The fare calculation request to process.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the final fare response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the state machine cannot process the request.</exception>
    Task<FareResponse> ProcessAsync(FareRequest request);

    /// <summary>
    /// Processes a fare calculation request through all states and returns the complete context with processing details.
    /// </summary>
    /// <param name="request">The fare calculation request to process.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the complete processing context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the state machine cannot process the request.</exception>
    Task<FareCalculationContext> ProcessWithContextAsync(FareRequest request);
}

/// <summary>
/// Implements a state machine that orchestrates fare calculation through a series of processing states.
/// Each state handles a specific aspect of the fare calculation workflow, ensuring proper separation of concerns.
/// </summary>
public class FareCalculationStateMachine : IFareCalculationStateMachine
{
    private readonly ILogger<FareCalculationStateMachine> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Func<IServiceProvider, IFareCalculationState>> _stateFactories;

    /// <summary>
    /// Initializes a new instance of the <see cref="FareCalculationStateMachine"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing state machine execution information.</param>
    /// <param name="serviceProvider">The service provider for resolving state dependencies.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or serviceProvider is null.</exception>
    public FareCalculationStateMachine(ILogger<FareCalculationStateMachine> logger, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        // Define the ordered sequence of states for fare calculation
        _stateFactories = new List<Func<IServiceProvider, IFareCalculationState>>
        {
            sp => (IFareCalculationState)sp.GetService(typeof(States.InitialCalculationState))!,
            sp => (IFareCalculationState)sp.GetService(typeof(States.BaseFareCalculationState))!,
            sp => (IFareCalculationState)sp.GetService(typeof(States.DiscountApplicationState))!,
            sp => (IFareCalculationState)sp.GetService(typeof(States.FinalCalculationState))!
        };
    }

    /// <summary>
    /// Processes a fare calculation request through all states and returns the final fare response.
    /// </summary>
    /// <param name="request">The fare calculation request to process.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the final fare response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when any state in the workflow fails to process the request.</exception>
    public async Task<FareResponse> ProcessAsync(FareRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var context = await ProcessWithContextAsync(request);
        return context.Response;
    }

    /// <summary>
    /// Processes a fare calculation request through all states and returns the complete context with processing details.
    /// This method provides full visibility into the state machine's execution process.
    /// </summary>
    /// <param name="request">The fare calculation request to process.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the complete processing context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when any state in the workflow fails to process the request.</exception>
    public async Task<FareCalculationContext> ProcessWithContextAsync(FareRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Starting fare calculation state machine for {Origin} to {Destination}",
            request.Origin.Name, request.Destination.Name);

        var context = new FareCalculationContext
        {
            Request = request
        };

        try
        {
            // Process through each state in the defined order
            foreach (var stateFactory in _stateFactories)
            {
                var state = stateFactory(_serviceProvider);
                context.CurrentState = state;

                _logger.LogDebug("Entering state: {StateName}", state.StateName);
                context.ProcessingLog.Add($"Entering state: {state.StateName}");

                context = await state.ProcessAsync(context);

                _logger.LogDebug("Completed state: {StateName}", state.StateName);
                context.ProcessingLog.Add($"Completed state: {state.StateName}");
            }

            _logger.LogInformation("Fare calculation state machine completed successfully. Final fare: ${FinalFare:F2}",
                context.Response.Amount);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in fare calculation state machine");
            context.ProcessingLog.Add($"Error occurred: {ex.Message}");
            throw;
        }
    }
} 