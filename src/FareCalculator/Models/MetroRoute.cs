namespace FareCalculator.Models;

/// <summary>
/// Represents a calculated route between two stations in the metro system.
/// </summary>
public class MetroRoute
{
    /// <summary>
    /// Gets or sets the origin station of the route.
    /// </summary>
    public Station Origin { get; set; } = null!;

    /// <summary>
    /// Gets or sets the destination station of the route.
    /// </summary>
    public Station Destination { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sequence of metro line segments for this route.
    /// </summary>
    /// <value>A list of route segments, each representing travel on a specific metro line.</value>
    public List<RouteSegment> Segments { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of transfers required for this route.
    /// </summary>
    /// <value>The number of times a passenger must change metro lines (0 for direct routes).</value>
    public int TransferCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total estimated travel time in minutes.
    /// </summary>
    /// <value>The estimated travel time including transfers and waiting time.</value>
    public int EstimatedTravelTimeMinutes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total distance of the route in kilometers.
    /// </summary>
    /// <value>The sum of distances for all segments in the route.</value>
    public double TotalDistanceKilometers { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets a value indicating whether this is a direct route (no transfers).
    /// </summary>
    /// <value>True if the route requires no transfers; otherwise, false.</value>
    public bool IsDirectRoute => TransferCount == 0;

    /// <summary>
    /// Gets or sets the list of transfer stations on this route.
    /// </summary>
    /// <value>Stations where passengers must transfer between metro lines.</value>
    public List<Station> TransferStations { get; set; } = new();
}

/// <summary>
/// Represents a segment of a metro route on a specific metro line.
/// </summary>
public class RouteSegment
{
    /// <summary>
    /// Gets or sets the metro line for this segment.
    /// </summary>
    public MetroLine MetroLine { get; set; } = null!;

    /// <summary>
    /// Gets or sets the starting station for this segment.
    /// </summary>
    public Station StartStation { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ending station for this segment.
    /// </summary>
    public Station EndStation { get; set; } = null!;

    /// <summary>
    /// Gets or sets the distance of this segment in kilometers.
    /// </summary>
    public double DistanceKilometers { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the estimated travel time for this segment in minutes.
    /// </summary>
    public int EstimatedTimeMinutes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of stations traveled in this segment.
    /// </summary>
    public int StationCount { get; set; } = 0;
} 