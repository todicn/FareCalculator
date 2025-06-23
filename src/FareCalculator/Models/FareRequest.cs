namespace FareCalculator.Models;

/// <summary>
/// Represents a request for fare calculation containing all necessary information for determining transit costs.
/// </summary>
public class FareRequest
{
    /// <summary>
    /// Gets or sets the origin station where the journey begins.
    /// </summary>
    /// <value>The starting station for the transit journey.</value>
    public Station Origin { get; set; } = new();

    /// <summary>
    /// Gets or sets the destination station where the journey ends.
    /// </summary>
    /// <value>The ending station for the transit journey.</value>
    public Station Destination { get; set; } = new();

    /// <summary>
    /// Gets or sets the type of passenger making the journey.
    /// </summary>
    /// <value>The passenger classification used to determine applicable discounts and fare rules.</value>
    public PassengerType PassengerType { get; set; } = PassengerType.Adult;

    /// <summary>
    /// Gets or sets the planned date and time of travel.
    /// </summary>
    /// <value>The travel date and time used for applying time-based fare rules such as peak hour surcharges.</value>
    public DateTime TravelDate { get; set; } = DateTime.Now;
} 