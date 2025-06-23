namespace FareCalculator.Models;

/// <summary>
/// Represents a metro station with its location, zone information, and geographical coordinates.
/// </summary>
public class Station
{
    /// <summary>
    /// Gets or sets the unique identifier for the station.
    /// </summary>
    /// <value>A positive integer that uniquely identifies the station within the metro system.</value>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the station.
    /// </summary>
    /// <value>The human-readable name of the station as displayed to passengers.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fare zone designation for the station.
    /// </summary>
    /// <value>A string identifier (e.g., "A", "B", "C") indicating which fare zone the station belongs to.</value>
    public string Zone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latitude coordinate of the station's geographical location.
    /// </summary>
    /// <value>The latitude in decimal degrees, used for distance calculations between stations.</value>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate of the station's geographical location.
    /// </summary>
    /// <value>The longitude in decimal degrees, used for distance calculations between stations.</value>
    public double Longitude { get; set; }
} 