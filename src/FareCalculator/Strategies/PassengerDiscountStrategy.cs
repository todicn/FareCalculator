using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FareCalculator.Strategies;

public class PassengerDiscountStrategy : IDiscountStrategy
{
    private readonly ILogger<PassengerDiscountStrategy> _logger;
    private readonly FareCalculationOptions _options;

    public string StrategyName => "Passenger Type Discount";
    public int Priority => _options.Priorities.PassengerDiscountStrategy;

    public PassengerDiscountStrategy(
        ILogger<PassengerDiscountStrategy> logger,
        IOptions<FareCalculationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public decimal ApplyDiscount(decimal baseFare, FareRequest request)
    {
        var discount = _options.GetPassengerDiscount(request.PassengerType);
        var discountedFare = baseFare * (1 - discount);

        _logger.LogInformation("Applied {PassengerType} discount: {Discount:P0} on ${BaseFare:F2} = ${DiscountedFare:F2}",
            request.PassengerType, discount, baseFare, discountedFare);

        return discountedFare;
    }

    public bool AppliesTo(PassengerType passengerType)
    {
        return _options.PassengerDiscounts.ContainsKey(passengerType.ToString());
    }
}

public class TimeBasedDiscountStrategy : IDiscountStrategy
{
    private readonly ILogger<TimeBasedDiscountStrategy> _logger;
    private readonly FareCalculationOptions _options;

    public string StrategyName => "Time-Based Discount";
    public int Priority => _options.Priorities.TimeBasedDiscountStrategy;

    public TimeBasedDiscountStrategy(
        ILogger<TimeBasedDiscountStrategy> logger,
        IOptions<FareCalculationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public decimal ApplyDiscount(decimal baseFare, FareRequest request)
    {
        var multiplier = GetTimeMultiplier(request.TravelDate);
        var adjustedFare = baseFare * multiplier;

        _logger.LogInformation("Applied time-based adjustment: {Multiplier:P0} on ${BaseFare:F2} = ${AdjustedFare:F2}",
            multiplier - 1, baseFare, adjustedFare);

        return adjustedFare;
    }

    public bool AppliesTo(PassengerType passengerType)
    {
        return true; // Applies to all passenger types
    }

    private decimal GetTimeMultiplier(DateTime travelTime)
    {
        var peakHours = _options.TimeBasedRules.PeakHours;
        var offPeakHours = _options.TimeBasedRules.OffPeakHours;

        // Peak hours: configurable weekday hours
        var isPeakHour = IsWeekday(travelTime) && 
                        (IsBetweenHours(travelTime, peakHours.WeekdayMorningStart, peakHours.WeekdayMorningEnd) || 
                         IsBetweenHours(travelTime, peakHours.WeekdayEveningStart, peakHours.WeekdayEveningEnd));
        
        if (isPeakHour) return 1 + peakHours.Surcharge; // Configurable surcharge

        // Off-peak: configurable night hours
        var isOffPeak = IsBetweenHours(travelTime, offPeakHours.NightStart, 24) || 
                       IsBetweenHours(travelTime, 0, offPeakHours.NightEnd);
        if (isOffPeak) return 1 - offPeakHours.Discount; // Configurable discount

        return 1.00m; // Regular hours
    }

    private static bool IsWeekday(DateTime dateTime) => 
        dateTime.DayOfWeek >= DayOfWeek.Monday && dateTime.DayOfWeek <= DayOfWeek.Friday;

    private static bool IsBetweenHours(DateTime dateTime, int startHour, int endHour)
    {
        var hour = dateTime.Hour;
        return startHour <= endHour ? 
            hour >= startHour && hour < endHour : 
            hour >= startHour || hour < endHour;
    }
} 