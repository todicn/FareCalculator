using FareCalculator.Models;

namespace FareCalculator.Interfaces;

/// <summary>
/// Defines the contract for fare calculation strategies that can compute base fares using different algorithms.
/// Implements the Strategy pattern to allow runtime selection of fare calculation methods.
/// </summary>
public interface IFareCalculationStrategy
{
    /// <summary>
    /// Gets the unique name of this fare calculation strategy.
    /// </summary>
    /// <value>A descriptive name identifying the strategy (e.g., "ZoneBased", "DistanceBased").</value>
    string StrategyName { get; }

    /// <summary>
    /// Calculates the base fare for a transit journey asynchronously using this strategy's specific algorithm.
    /// </summary>
    /// <param name="request">The fare calculation request containing journey details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the calculated base fare amount.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the strategy cannot handle the given request.</exception>
    Task<decimal> CalculateBaseFareAsync(FareRequest request);

    /// <summary>
    /// Determines whether this strategy can handle the specified fare calculation request.
    /// </summary>
    /// <param name="request">The fare calculation request to evaluate.</param>
    /// <returns>True if this strategy can process the request; otherwise, false.</returns>
    bool CanHandle(FareRequest request);

    /// <summary>
    /// Gets the priority of this strategy for selection when multiple strategies can handle a request.
    /// Higher values indicate higher priority.
    /// </summary>
    /// <value>An integer representing the strategy's priority. Higher numbers are selected first.</value>
    int Priority { get; }
} 