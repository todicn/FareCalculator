using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FareCalculator.Services;

/// <summary>
/// Implements business rules for fare processing including passenger discounts, time-based adjustments, and zone calculations.
/// Contains the core logic for applying various fare rules and policies in the metro transit system.
/// </summary>
public class FareRuleEngine : IFareRuleEngine
{
    private readonly ILogger<FareRuleEngine> _logger;
    private readonly FareCalculationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FareRuleEngine"/> class with configuration-based settings.
    /// </summary>
    /// <param name="logger">The logger for capturing rule engine execution information.</param>
    /// <param name="options">Configuration options for fare calculation settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or options is null.</exception>
    public FareRuleEngine(ILogger<FareRuleEngine> logger, IOptions<FareCalculationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Applies passenger-type-based discounts to the base fare according to business rules.
    /// </summary>
    /// <param name="baseFare">The original base fare amount before discount application.</param>
    /// <param name="passengerType">The type of passenger for which to calculate discounts.</param>
    /// <returns>The fare amount after applying applicable passenger discounts.</returns>
    /// <exception cref="ArgumentException">Thrown when baseFare is negative.</exception>
    public decimal ApplyDiscounts(decimal baseFare, PassengerType passengerType)
    {
        if (baseFare < 0)
            throw new ArgumentException("Base fare cannot be negative.", nameof(baseFare));

        _logger.LogInformation("Applying discount for passenger type: {PassengerType}", passengerType);
        
        var discount = _options.GetPassengerDiscount(passengerType);
        var discountedFare = baseFare * (1 - discount);
        
        _logger.LogInformation("Base fare: {BaseFare}, Discount: {Discount}%, Final fare: {FinalFare}", 
            baseFare, discount * 100, discountedFare);
        
        return discountedFare;
    }

    /// <summary>
    /// Applies time-based fare adjustments such as peak hour surcharges or off-peak discounts.
    /// Peak hours are 7-9 AM and 5-7 PM on weekdays with 25% surcharge.
    /// Off-peak hours are 10 PM - 6 AM with 10% discount.
    /// </summary>
    /// <param name="baseFare">The original base fare amount before time-based adjustments.</param>
    /// <param name="travelTime">The date and time of the planned travel.</param>
    /// <returns>The fare amount after applying time-based rules (surcharges or discounts).</returns>
    /// <exception cref="ArgumentException">Thrown when baseFare is negative.</exception>
    public decimal ApplyTimeBasedRules(decimal baseFare, DateTime travelTime)
    {
        if (baseFare < 0)
            throw new ArgumentException("Base fare cannot be negative.", nameof(baseFare));

        _logger.LogInformation("Applying time-based rules for travel time: {TravelTime}", travelTime);
        
        var peakHours = _options.TimeBasedRules.PeakHours;
        var offPeakHours = _options.TimeBasedRules.OffPeakHours;
        
        // Peak hours: configurable weekday hours
        var isPeakHour = IsWeekday(travelTime) && 
                        (IsBetweenHours(travelTime, peakHours.WeekdayMorningStart, peakHours.WeekdayMorningEnd) || 
                         IsBetweenHours(travelTime, peakHours.WeekdayEveningStart, peakHours.WeekdayEveningEnd));
        
        if (isPeakHour)
        {
            var peakFare = baseFare * (1 + peakHours.Surcharge); // Configurable surcharge during peak hours
            _logger.LogInformation("Peak hour surcharge applied: {PeakFare}", peakFare);
            return peakFare;
        }
        
        // Off-peak discount: configurable discount during late night hours
        var isOffPeak = IsBetweenHours(travelTime, offPeakHours.NightStart, 24) || 
                       IsBetweenHours(travelTime, 0, offPeakHours.NightEnd);
        if (isOffPeak)
        {
            var offPeakFare = baseFare * (1 - offPeakHours.Discount); // Configurable discount during off-peak hours
            _logger.LogInformation("Off-peak discount applied: {OffPeakFare}", offPeakFare);
            return offPeakFare;
        }
        
        return baseFare;
    }

    /// <summary>
    /// Calculates the number of fare zones that will be traversed between the origin and destination stations.
    /// Zone A = 1, Zone B = 2, Zone C = 3. The calculation includes both origin and destination zones.
    /// </summary>
    /// <param name="origin">The starting station of the journey.</param>
    /// <param name="destination">The ending station of the journey.</param>
    /// <returns>The number of zones that will be crossed during the journey, used for fare calculation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    public int CalculateNumberOfZones(Station origin, Station destination)
    {
        if (origin == null)
            throw new ArgumentNullException(nameof(origin));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        _logger.LogInformation("Calculating zones between {Origin} ({OriginZone}) and {Destination} ({DestinationZone})", 
            origin.Name, origin.Zone, destination.Name, destination.Zone);
        
        var originZoneNumber = _options.GetZoneValue(origin.Zone);
        var destinationZoneNumber = _options.GetZoneValue(destination.Zone);
        
        var numberOfZones = Math.Abs(destinationZoneNumber - originZoneNumber) + 1;
        
        _logger.LogInformation("Number of zones: {NumberOfZones}", numberOfZones);
        return numberOfZones;
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