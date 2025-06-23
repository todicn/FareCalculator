using FareCalculator.Models;

namespace FareCalculator.Interfaces;

/// <summary>
/// Provides services for calculating metro transit fares based on various parameters including 
/// origin, destination, passenger type, and travel time.
/// </summary>
public interface IFareCalculationService
{
    /// <summary>
    /// Calculates the fare for a metro transit journey asynchronously.
    /// </summary>
    /// <param name="request">The fare calculation request containing origin, destination, passenger type, and travel date.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the calculated fare response with amount, currency, and additional details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no suitable calculation strategy can be found for the request.</exception>
    Task<FareResponse> CalculateFareAsync(FareRequest request);
} 