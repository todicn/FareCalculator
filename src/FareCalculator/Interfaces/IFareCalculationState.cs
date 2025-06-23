using FareCalculator.Models;

namespace FareCalculator.Interfaces;

/// <summary>
/// Defines the contract for states in the fare calculation state machine.
/// Implements the State pattern to manage the fare calculation workflow through distinct processing phases.
/// </summary>
public interface IFareCalculationState
{
    /// <summary>
    /// Gets the unique name of this fare calculation state.
    /// </summary>
    /// <value>A descriptive name identifying the state (e.g., "InitialCalculation", "DiscountApplication").</value>
    string StateName { get; }

    /// <summary>
    /// Processes the fare calculation context according to this state's specific logic and responsibilities.
    /// </summary>
    /// <param name="context">The fare calculation context containing request data, current state, and processing results.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated context after processing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the context parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the state cannot process the current context.</exception>
    Task<FareCalculationContext> ProcessAsync(FareCalculationContext context);

    /// <summary>
    /// Determines whether this state can transition to the specified next state according to the state machine rules.
    /// </summary>
    /// <param name="nextState">The proposed next state in the workflow.</param>
    /// <returns>True if the transition is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the nextState parameter is null.</exception>
    bool CanTransitionTo(IFareCalculationState nextState);
}

/// <summary>
/// Represents the context object that carries data and state information throughout the fare calculation workflow.
/// This class maintains the complete state of a fare calculation process as it moves through different processing stages.
/// </summary>
public class FareCalculationContext
{
    /// <summary>
    /// Gets or sets the original fare calculation request containing journey details.
    /// </summary>
    /// <value>The fare request with origin, destination, passenger type, and travel date information.</value>
    public FareRequest Request { get; set; } = new();

    /// <summary>
    /// Gets or sets the fare response that will be populated during the calculation process.
    /// </summary>
    /// <value>The fare response containing the final calculated amount and related information.</value>
    public FareResponse Response { get; set; } = new();

    /// <summary>
    /// Gets or sets the current calculated fare amount during processing.
    /// </summary>
    /// <value>The fare amount as it progresses through different calculation stages.</value>
    public decimal CurrentFare { get; set; }

    /// <summary>
    /// Gets or sets the current state in the fare calculation state machine.
    /// </summary>
    /// <value>The currently active state responsible for processing the context.</value>
    public IFareCalculationState? CurrentState { get; set; }

    /// <summary>
    /// Gets or sets additional data storage for passing information between states.
    /// </summary>
    /// <value>A dictionary for storing intermediate calculation results, metadata, and state-specific data.</value>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Gets or sets a log of processing steps for audit trail and debugging purposes.
    /// </summary>
    /// <value>A list of log messages describing each step in the fare calculation process.</value>
    public List<string> ProcessingLog { get; set; } = new();
} 