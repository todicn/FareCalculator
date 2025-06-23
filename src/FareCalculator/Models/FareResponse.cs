namespace FareCalculator.Models;

/// <summary>
/// Represents the result of a fare calculation containing the final fare amount and related journey information.
/// </summary>
public class FareResponse
{
    /// <summary>
    /// Gets or sets the calculated fare amount for the journey.
    /// </summary>
    /// <value>The final fare amount after applying all applicable discounts and surcharges.</value>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code for the fare amount.
    /// </summary>
    /// <value>The three-letter currency code (e.g., "USD", "EUR") indicating the currency of the fare amount.</value>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the number of fare zones traversed during the journey.
    /// </summary>
    /// <value>The count of fare zones crossed, which is used in zone-based fare calculation strategies.</value>
    public int NumberOfZones { get; set; }

    /// <summary>
    /// Gets or sets the total distance of the journey in kilometers.
    /// </summary>
    /// <value>The geographical distance between origin and destination stations, used for distance-based calculations.</value>
    public double Distance { get; set; }

    /// <summary>
    /// Gets or sets the type of fare calculation used.
    /// </summary>
    /// <value>A descriptive string indicating which fare calculation method was applied (e.g., "Zone-based", "Distance-based").</value>
    public string FareType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional description or breakdown of the fare calculation.
    /// </summary>
    /// <value>Human-readable text providing details about how the fare was calculated, including any discounts or surcharges applied.</value>
    public string Description { get; set; } = string.Empty;
} 