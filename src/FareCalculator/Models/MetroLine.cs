namespace FareCalculator.Models;

/// <summary>
/// Represents a metro line with its characteristics and operational information.
/// </summary>
public class MetroLine
{
    /// <summary>
    /// Gets or sets the unique identifier for the metro line.
    /// </summary>
    /// <value>A positive integer that uniquely identifies the metro line within the system.</value>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the metro line.
    /// </summary>
    /// <value>The human-readable name of the metro line (e.g., "Red Line", "Blue Line").</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the short code or abbreviation for the metro line.
    /// </summary>
    /// <value>A short identifier for the metro line (e.g., "RL", "BL", "GL").</value>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color associated with the metro line for visual identification.
    /// </summary>
    /// <value>The hex color code or color name used to represent the line on maps and signage.</value>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the metro line is currently operational.
    /// </summary>
    /// <value>True if the line is operational; otherwise, false.</value>
    public bool IsOperational { get; set; } = true;

    /// <summary>
    /// Gets or sets the type of the metro line (e.g., "Express", "Local", "Shuttle").
    /// </summary>
    /// <value>A string describing the service type of the metro line.</value>
    public string LineType { get; set; } = "Local";

    /// <summary>
    /// Gets or sets the fare multiplier for this metro line.
    /// </summary>
    /// <value>A decimal multiplier applied to base fares for trips on this line (1.0 = normal fare, 1.5 = 50% premium).</value>
    public decimal FareMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the operating hours for the metro line.
    /// </summary>
    /// <value>A string describing the operating hours (e.g., "5:00 AM - 12:00 AM").</value>
    public string OperatingHours { get; set; } = "24/7";

    /// <summary>
    /// Gets or sets additional notes or information about the metro line.
    /// </summary>
    /// <value>Optional additional information about the line's characteristics or special services.</value>
    public string? Notes { get; set; }
} 