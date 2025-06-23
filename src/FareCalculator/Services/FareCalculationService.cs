using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;

namespace FareCalculator.Services;

/// <summary>
/// Provides fare calculation services using a state machine approach to process fare requests.
/// This service orchestrates the complete fare calculation workflow from initial request to final response.
/// </summary>
public class FareCalculationService : IFareCalculationService
{
    private readonly ILogger<FareCalculationService> _logger;
    private readonly IFareCalculationStateMachine _stateMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="FareCalculationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing service execution information.</param>
    /// <param name="stateMachine">The state machine responsible for orchestrating the fare calculation workflow.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or stateMachine is null.</exception>
    public FareCalculationService(
        ILogger<FareCalculationService> logger,
        IFareCalculationStateMachine stateMachine)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
    }

    /// <summary>
    /// Calculates the fare for a metro transit journey asynchronously using the state machine workflow.
    /// </summary>
    /// <param name="request">The fare calculation request containing journey details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the calculated fare response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the state machine cannot process the request.</exception>
    public async Task<FareResponse> CalculateFareAsync(FareRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Starting fare calculation for {Origin} to {Destination} for {PassengerType} on {TravelDate}",
            request.Origin.Name, request.Destination.Name, request.PassengerType, request.TravelDate);

        try
        {
            var response = await _stateMachine.ProcessAsync(request);
            
            _logger.LogInformation("Fare calculation completed. Final fare: {FinalFare}", response.Amount);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in fare calculation for request: {@Request}", request);
            throw;
        }
    }
} 