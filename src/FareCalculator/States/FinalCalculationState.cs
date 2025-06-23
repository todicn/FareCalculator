using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;

namespace FareCalculator.States;

/// <summary>
/// Represents the final calculation state that completes the fare calculation process.
/// This state finalizes the fare amount, calculates zone information, and generates descriptive information.
/// </summary>
public class FinalCalculationState : IFareCalculationState
{
    private readonly ILogger<FinalCalculationState> _logger;
    private readonly IFareRuleEngine _fareRuleEngine;

    /// <summary>
    /// Gets the name of this state in the fare calculation workflow.
    /// </summary>
    /// <value>Returns "FinalCalculation" to identify this as the final calculation state.</value>
    public string StateName => "FinalCalculation";

    /// <summary>
    /// Initializes a new instance of the <see cref="FinalCalculationState"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing state execution information.</param>
    /// <param name="fareRuleEngine">The fare rule engine for calculating zones and applying final adjustments.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public FinalCalculationState(ILogger<FinalCalculationState> logger, IFareRuleEngine fareRuleEngine)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fareRuleEngine = fareRuleEngine ?? throw new ArgumentNullException(nameof(fareRuleEngine));
    }

    /// <summary>
    /// Processes the fare calculation context by finalizing the fare amount and generating complete response information.
    /// Rounds the final fare, calculates zone information, and creates descriptive text for the journey.
    /// </summary>
    /// <param name="context">The fare calculation context containing the processed fare information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the completed context with final response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the context parameter is null.</exception>
    public Task<FareCalculationContext> ProcessAsync(FareCalculationContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _logger.LogInformation("Finalizing fare calculation");

        // Round the final fare to two decimal places
        context.Response.Amount = Math.Round(context.CurrentFare, 2);
        
        // Calculate and set additional response properties
        context.Response.NumberOfZones = _fareRuleEngine.CalculateNumberOfZones(
            context.Request.Origin, context.Request.Destination);
        
        context.Response.FareType = GetFareTypeDescription(context.Request);
        context.Response.Description = GenerateDescription(context);

        // Add final logging entries
        context.ProcessingLog.Add($"Final fare: ${context.Response.Amount:F2}");
        context.ProcessingLog.Add($"Calculation completed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        _logger.LogInformation("Fare calculation completed. Final amount: ${FinalAmount:F2}", 
            context.Response.Amount);

        return Task.FromResult(context);
    }

    /// <summary>
    /// Determines whether this state can transition to the specified next state.
    /// The final calculation state cannot transition to any other state as it represents the end of the workflow.
    /// </summary>
    /// <param name="nextState">The proposed next state in the workflow.</param>
    /// <returns>Always returns false as this is the terminal state in the workflow.</returns>
    public bool CanTransitionTo(IFareCalculationState nextState)
    {
        return false; // This is the final state
    }

    /// <summary>
    /// Generates a descriptive fare type string based on passenger type and travel time.
    /// </summary>
    /// <param name="request">The fare request containing passenger and travel information.</param>
    /// <returns>A string describing the fare type (e.g., "Adult - Peak Hours", "Child - Off-Peak Hours").</returns>
    private string GetFareTypeDescription(FareRequest request)
    {
        var passengerType = request.PassengerType.ToString();
        var timeDescription = GetTimeDescription(request.TravelDate);
        return $"{passengerType} - {timeDescription}";
    }

    /// <summary>
    /// Determines the time-based description for the travel period.
    /// </summary>
    /// <param name="travelDate">The date and time of travel.</param>
    /// <returns>A string describing the time period ("Peak Hours", "Off-Peak Hours", or "Regular Hours").</returns>
    private static string GetTimeDescription(DateTime travelDate)
    {
        var isPeakHour = IsWeekday(travelDate) && 
                        (IsBetweenHours(travelDate, 7, 9) || IsBetweenHours(travelDate, 17, 19));
        
        if (isPeakHour) return "Peak Hours";
        
        var isOffPeak = IsBetweenHours(travelDate, 22, 24) || IsBetweenHours(travelDate, 0, 6);
        if (isOffPeak) return "Off-Peak Hours";
        
        return "Regular Hours";
    }

    /// <summary>
    /// Generates a comprehensive description of the fare calculation including route, strategy, and adjustments.
    /// </summary>
    /// <param name="context">The fare calculation context containing all processing information.</param>
    /// <returns>A detailed description of how the fare was calculated.</returns>
    private string GenerateDescription(FareCalculationContext context)
    {
        var strategyUsed = context.Data.GetValueOrDefault("StrategyUsed", "Unknown");
        var baseFare = context.Data.GetValueOrDefault("BaseFare", 0m);
        
        var description = $"Journey from {context.Request.Origin.Name} to {context.Request.Destination.Name}. " +
                         $"Calculated using {strategyUsed} strategy";

        if (context.Response.NumberOfZones > 0)
        {
            description += $", covering {context.Response.NumberOfZones} zone(s)";
        }

        description += $". Base fare: ${baseFare:F2}";

        if (Math.Abs(context.Response.Amount - (decimal)baseFare) > 0.01m)
        {
            description += $", Final fare after adjustments: ${context.Response.Amount:F2}";
        }

        return description;
    }

    /// <summary>
    /// Determines whether the specified date is a weekday (Monday through Friday).
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns>True if the date is a weekday; otherwise, false.</returns>
    private static bool IsWeekday(DateTime dateTime) => 
        dateTime.DayOfWeek >= DayOfWeek.Monday && dateTime.DayOfWeek <= DayOfWeek.Friday;

    /// <summary>
    /// Determines whether the specified time falls within the given hour range.
    /// Handles both same-day ranges (e.g., 9-17) and overnight ranges (e.g., 22-6).
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <param name="startHour">The starting hour of the range (0-23).</param>
    /// <param name="endHour">The ending hour of the range (0-24).</param>
    /// <returns>True if the time falls within the specified range; otherwise, false.</returns>
    private static bool IsBetweenHours(DateTime dateTime, int startHour, int endHour)
    {
        var hour = dateTime.Hour;
        return startHour <= endHour ? 
            hour >= startHour && hour < endHour : 
            hour >= startHour || hour < endHour;
    }
} 