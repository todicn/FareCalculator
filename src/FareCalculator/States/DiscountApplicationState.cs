using FareCalculator.Interfaces;
using Microsoft.Extensions.Logging;

namespace FareCalculator.States;

/// <summary>
/// Represents the discount application state that applies all applicable discount strategies to the base fare.
/// This state processes discounts in priority order and tracks the total discount amount applied.
/// </summary>
public class DiscountApplicationState : IFareCalculationState
{
    private readonly ILogger<DiscountApplicationState> _logger;
    private readonly IEnumerable<IDiscountStrategy> _discountStrategies;

    /// <summary>
    /// Gets the name of this state in the fare calculation workflow.
    /// </summary>
    /// <value>Returns "DiscountApplication" to identify this as the discount application state.</value>
    public string StateName => "DiscountApplication";

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscountApplicationState"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing state execution information.</param>
    /// <param name="discountStrategies">The collection of available discount strategies.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public DiscountApplicationState(
        ILogger<DiscountApplicationState> logger,
        IEnumerable<IDiscountStrategy> discountStrategies)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _discountStrategies = discountStrategies ?? throw new ArgumentNullException(nameof(discountStrategies));
    }

    /// <summary>
    /// Processes the fare calculation context by applying all applicable discount strategies.
    /// Strategies are applied in priority order (highest priority first) and the total discount is tracked.
    /// </summary>
    /// <param name="context">The fare calculation context containing the base fare and passenger details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated context with discounted fare.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the context parameter is null.</exception>
    public Task<FareCalculationContext> ProcessAsync(FareCalculationContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _logger.LogInformation("Applying discount strategies");

        // Find all applicable discount strategies for the passenger type
        var applicableStrategies = _discountStrategies
            .Where(s => s.AppliesTo(context.Request.PassengerType))
            .OrderByDescending(s => s.Priority)
            .ToList();

        context.ProcessingLog.Add($"Found {applicableStrategies.Count} applicable discount strategies");

        var originalFare = context.CurrentFare;
        
        // Apply each applicable discount strategy in priority order
        foreach (var strategy in applicableStrategies)
        {
            var previousFare = context.CurrentFare;
            context.CurrentFare = strategy.ApplyDiscount(context.CurrentFare, context.Request);
            
            context.ProcessingLog.Add($"Applied {strategy.StrategyName}: ${previousFare:F2} → ${context.CurrentFare:F2}");
            
            _logger.LogInformation("Applied {StrategyName}: ${PreviousFare:F2} → ${NewFare:F2}",
                strategy.StrategyName, previousFare, context.CurrentFare);
        }

        // Store discount calculation details for reference
        context.Data["OriginalFare"] = originalFare;
        context.Data["FareAfterDiscounts"] = context.CurrentFare;
        context.Data["TotalDiscount"] = originalFare - context.CurrentFare;

        _logger.LogInformation("Total discount applied: ${TotalDiscount:F2} (${OriginalFare:F2} → ${FinalFare:F2})",
            originalFare - context.CurrentFare, originalFare, context.CurrentFare);

        return Task.FromResult(context);
    }

    /// <summary>
    /// Determines whether this state can transition to the specified next state.
    /// The discount application state can only transition to the final calculation state.
    /// </summary>
    /// <param name="nextState">The proposed next state in the workflow.</param>
    /// <returns>True if the next state is the final calculation state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the nextState parameter is null.</exception>
    public bool CanTransitionTo(IFareCalculationState nextState)
    {
        if (nextState == null)
            throw new ArgumentNullException(nameof(nextState));

        return nextState.StateName == "FinalCalculation";
    }
} 