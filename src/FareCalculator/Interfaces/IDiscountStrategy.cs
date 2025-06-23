using FareCalculator.Models;

namespace FareCalculator.Interfaces;

/// <summary>
/// Defines the contract for discount strategies that can apply various types of discounts to base fares.
/// Implements the Strategy pattern to allow flexible discount application based on passenger type and other criteria.
/// </summary>
public interface IDiscountStrategy
{
    /// <summary>
    /// Gets the unique name of this discount strategy.
    /// </summary>
    /// <value>A descriptive name identifying the strategy (e.g., "PassengerDiscount", "TimeBasedDiscount").</value>
    string StrategyName { get; }

    /// <summary>
    /// Applies this discount strategy to the specified base fare for the given request.
    /// </summary>
    /// <param name="baseFare">The original base fare amount before discount application.</param>
    /// <param name="request">The fare calculation request containing passenger type and travel details.</param>
    /// <returns>The fare amount after applying the discount. Returns the original amount if no discount applies.</returns>
    /// <exception cref="ArgumentException">Thrown when baseFare is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    decimal ApplyDiscount(decimal baseFare, FareRequest request);

    /// <summary>
    /// Determines whether this discount strategy applies to the specified passenger type.
    /// </summary>
    /// <param name="passengerType">The type of passenger to evaluate for discount eligibility.</param>
    /// <returns>True if this discount strategy applies to the passenger type; otherwise, false.</returns>
    bool AppliesTo(PassengerType passengerType);

    /// <summary>
    /// Gets the priority of this discount strategy for application when multiple strategies apply.
    /// Higher values indicate higher priority and are applied first.
    /// </summary>
    /// <value>An integer representing the strategy's priority. Higher numbers are applied first.</value>
    int Priority { get; }
} 