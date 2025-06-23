using FareCalculator.Models;

namespace FareCalculator.Interfaces;

/// <summary>
/// Provides rule-based fare processing services including discount application and time-based fare adjustments.
/// Implements business rules for fare calculations in the metro transit system.
/// </summary>
public interface IFareRuleEngine
{
    /// <summary>
    /// Applies passenger-type-based discounts to the base fare according to business rules.
    /// </summary>
    /// <param name="baseFare">The original base fare amount before discount application.</param>
    /// <param name="passengerType">The type of passenger for which to calculate discounts.</param>
    /// <returns>The fare amount after applying applicable passenger discounts.</returns>
    /// <exception cref="ArgumentException">Thrown when baseFare is negative.</exception>
    decimal ApplyDiscounts(decimal baseFare, PassengerType passengerType);

    /// <summary>
    /// Applies time-based fare adjustments such as peak hour surcharges or off-peak discounts.
    /// </summary>
    /// <param name="baseFare">The original base fare amount before time-based adjustments.</param>
    /// <param name="travelTime">The date and time of the planned travel.</param>
    /// <returns>The fare amount after applying time-based rules (surcharges or discounts).</returns>
    /// <exception cref="ArgumentException">Thrown when baseFare is negative.</exception>
    decimal ApplyTimeBasedRules(decimal baseFare, DateTime travelTime);

    /// <summary>
    /// Calculates the number of fare zones that will be traversed between the origin and destination stations.
    /// </summary>
    /// <param name="origin">The starting station of the journey.</param>
    /// <param name="destination">The ending station of the journey.</param>
    /// <returns>The number of zones that will be crossed during the journey, used for fare calculation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    int CalculateNumberOfZones(Station origin, Station destination);
} 