using FareCalculator.Models;

namespace FareCalculator.Configuration;

/// <summary>
/// Configuration options for fare calculation settings.
/// </summary>
public class FareCalculationOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "FareCalculation";

    /// <summary>
    /// Gets or sets the default currency for fare amounts.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets zone-based fare amounts.
    /// </summary>
    public Dictionary<int, decimal> ZoneBasedFares { get; set; } = new();

    /// <summary>
    /// Gets or sets distance-based fare configuration.
    /// </summary>
    public DistanceBasedFareOptions DistanceBasedFares { get; set; } = new();

    /// <summary>
    /// Gets or sets passenger discount percentages by type.
    /// </summary>
    public Dictionary<string, decimal> PassengerDiscounts { get; set; } = new();

    /// <summary>
    /// Gets or sets time-based fare adjustment rules.
    /// </summary>
    public TimeBasedRulesOptions TimeBasedRules { get; set; } = new();

    /// <summary>
    /// Gets or sets zone to numeric mapping.
    /// </summary>
    public Dictionary<string, int> ZoneMapping { get; set; } = new();

    /// <summary>
    /// Gets or sets strategy priority values.
    /// </summary>
    public StrategyPriorityOptions Priorities { get; set; } = new();

    /// <summary>
    /// Gets the discount percentage for a passenger type.
    /// </summary>
    /// <param name="passengerType">The passenger type.</param>
    /// <returns>The discount percentage (0.0 to 1.0).</returns>
    public decimal GetPassengerDiscount(PassengerType passengerType)
    {
        return PassengerDiscounts.GetValueOrDefault(passengerType.ToString(), 0.00m);
    }

    /// <summary>
    /// Gets the zone-based fare for a number of zones.
    /// </summary>
    /// <param name="numberOfZones">The number of zones traveled.</param>
    /// <returns>The base fare amount.</returns>
    public decimal GetZoneBasedFare(int numberOfZones)
    {
        return ZoneBasedFares.GetValueOrDefault(numberOfZones, ZoneBasedFares.Values.Max());
    }

    /// <summary>
    /// Gets the numeric zone value for a zone letter.
    /// </summary>
    /// <param name="zone">The zone letter (e.g., "A", "B", "C").</param>
    /// <returns>The numeric zone value.</returns>
    public int GetZoneValue(string zone)
    {
        return ZoneMapping.GetValueOrDefault(zone, 1);
    }
}

/// <summary>
/// Configuration options for distance-based fare calculation.
/// </summary>
public class DistanceBasedFareOptions
{
    /// <summary>
    /// Gets or sets the base fare amount before distance calculation.
    /// </summary>
    public decimal BaseFare { get; set; } = 1.50m;

    /// <summary>
    /// Gets or sets the rate per kilometer.
    /// </summary>
    public decimal PerKilometerRate { get; set; } = 0.25m;
}

/// <summary>
/// Configuration options for time-based fare rules.
/// </summary>
public class TimeBasedRulesOptions
{
    /// <summary>
    /// Gets or sets peak hour configuration.
    /// </summary>
    public PeakHourOptions PeakHours { get; set; } = new();

    /// <summary>
    /// Gets or sets off-peak hour configuration.
    /// </summary>
    public OffPeakHourOptions OffPeakHours { get; set; } = new();
}

/// <summary>
/// Configuration options for peak hour rules.
/// </summary>
public class PeakHourOptions
{
    /// <summary>
    /// Gets or sets the surcharge percentage for peak hours.
    /// </summary>
    public decimal Surcharge { get; set; } = 0.25m;

    /// <summary>
    /// Gets or sets the weekday morning peak start hour.
    /// </summary>
    public int WeekdayMorningStart { get; set; } = 7;

    /// <summary>
    /// Gets or sets the weekday morning peak end hour.
    /// </summary>
    public int WeekdayMorningEnd { get; set; } = 9;

    /// <summary>
    /// Gets or sets the weekday evening peak start hour.
    /// </summary>
    public int WeekdayEveningStart { get; set; } = 17;

    /// <summary>
    /// Gets or sets the weekday evening peak end hour.
    /// </summary>
    public int WeekdayEveningEnd { get; set; } = 19;
}

/// <summary>
/// Configuration options for off-peak hour rules.
/// </summary>
public class OffPeakHourOptions
{
    /// <summary>
    /// Gets or sets the discount percentage for off-peak hours.
    /// </summary>
    public decimal Discount { get; set; } = 0.10m;

    /// <summary>
    /// Gets or sets the night hours start time.
    /// </summary>
    public int NightStart { get; set; } = 22;

    /// <summary>
    /// Gets or sets the night hours end time.
    /// </summary>
    public int NightEnd { get; set; } = 6;
}

/// <summary>
/// Configuration options for strategy priorities.
/// </summary>
public class StrategyPriorityOptions
{
    /// <summary>
    /// Gets or sets the zone-based fare strategy priority.
    /// </summary>
    public int ZoneBasedFareStrategy { get; set; } = 100;

    /// <summary>
    /// Gets or sets the distance-based fare strategy priority.
    /// </summary>
    public int DistanceBasedFareStrategy { get; set; } = 50;

    /// <summary>
    /// Gets or sets the passenger discount strategy priority.
    /// </summary>
    public int PassengerDiscountStrategy { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time-based discount strategy priority.
    /// </summary>
    public int TimeBasedDiscountStrategy { get; set; } = 90;

    /// <summary>
    /// Gets or sets the metro line fare strategy priority.
    /// </summary>
    public int MetroLineFareStrategy { get; set; } = 110;
}

/// <summary>
/// Configuration options for geographical calculations.
/// </summary>
public class GeographyOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Geography";

    /// <summary>
    /// Gets or sets the Earth's radius in kilometers for distance calculations.
    /// </summary>
    public double EarthRadiusKilometers { get; set; } = 6371.0;
}

/// <summary>
/// Configuration options for station data.
/// </summary>
public class StationOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Stations";

    /// <summary>
    /// Gets or sets the list of configured stations.
    /// </summary>
    public List<Station> Stations { get; set; } = new();
}

/// <summary>
/// Configuration options for metro line data.
/// </summary>
public class MetroLineOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "MetroLines";

    /// <summary>
    /// Gets or sets the list of metro lines in the system.
    /// </summary>
    public List<MetroLine> Lines { get; set; } = new();

    /// <summary>
    /// Gets or sets line-specific fare multipliers.
    /// </summary>
    public Dictionary<string, decimal> LineFareMultipliers { get; set; } = new();

    /// <summary>
    /// Gets or sets transfer penalties between different metro lines.
    /// </summary>
    public Dictionary<string, decimal> TransferPenalties { get; set; } = new();

    /// <summary>
    /// Gets the fare multiplier for a specific metro line code.
    /// </summary>
    /// <param name="lineCode">The metro line code.</param>
    /// <returns>The fare multiplier for the line (default 1.0).</returns>
    public decimal GetLineFareMultiplier(string lineCode)
    {
        return LineFareMultipliers.GetValueOrDefault(lineCode, 1.0m);
    }

    /// <summary>
    /// Gets the transfer penalty for switching between metro lines.
    /// </summary>
    /// <param name="fromLineCode">The origin line code.</param>
    /// <param name="toLineCode">The destination line code.</param>
    /// <returns>The transfer penalty amount (default 0.0).</returns>
    public decimal GetTransferPenalty(string fromLineCode, string toLineCode)
    {
        var key = $"{fromLineCode}-{toLineCode}";
        return TransferPenalties.GetValueOrDefault(key, 0.0m);
    }
} 