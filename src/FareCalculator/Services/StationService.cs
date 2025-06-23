using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FareCalculator.Services;

/// <summary>
/// Provides station management services including station lookup, retrieval, and distance calculations.
/// Uses configuration-based station data and geographical constants for calculations.
/// </summary>
public class StationService : IStationService
{
    private readonly ILogger<StationService> _logger;
    private readonly List<Station> _stations;
    private readonly GeographyOptions _geographyOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="StationService"/> class with configuration-based settings.
    /// </summary>
    /// <param name="logger">The logger for capturing service execution information.</param>
    /// <param name="stationOptions">Configuration options for station data.</param>
    /// <param name="geographyOptions">Configuration options for geographical calculations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public StationService(
        ILogger<StationService> logger,
        IOptions<List<Station>> stationOptions,
        IOptions<GeographyOptions> geographyOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stations = stationOptions?.Value ?? throw new ArgumentNullException(nameof(stationOptions));
        _geographyOptions = geographyOptions?.Value ?? throw new ArgumentNullException(nameof(geographyOptions));
    }

    /// <summary>
    /// Retrieves a station by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the station if found, otherwise null.</returns>
    public Task<Station?> GetStationByIdAsync(int id)
    {
        _logger.LogInformation("Getting station by ID: {Id}", id);
        var station = _stations.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(station);
    }

    /// <summary>
    /// Retrieves a station by its name asynchronously using case-insensitive search.
    /// </summary>
    /// <param name="name">The name of the station to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the station if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when the name parameter is null or empty.</exception>
    public Task<Station?> GetStationByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Station name cannot be null or empty.", nameof(name));

        _logger.LogInformation("Getting station by name: {Name}", name);
        var station = _stations.FirstOrDefault(s => 
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(station);
    }

    /// <summary>
    /// Retrieves all available stations in the metro system asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all stations.</returns>
    public Task<IEnumerable<Station>> GetAllStationsAsync()
    {
        _logger.LogInformation("Getting all stations");
        return Task.FromResult<IEnumerable<Station>>(_stations);
    }

    /// <summary>
    /// Calculates the distance between two stations in kilometers using the Haversine formula.
    /// </summary>
    /// <param name="origin">The origin station.</param>
    /// <param name="destination">The destination station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the distance in kilometers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    public Task<double> CalculateDistanceAsync(Station origin, Station destination)
    {
        if (origin == null)
            throw new ArgumentNullException(nameof(origin));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        _logger.LogInformation("Calculating distance between {Origin} and {Destination}", 
            origin.Name, destination.Name);
        
        // Using Haversine formula for distance calculation
        var distance = CalculateHaversineDistance(
            origin.Latitude, origin.Longitude, 
            destination.Latitude, destination.Longitude);
        
        return Task.FromResult(distance);
    }

    /// <summary>
    /// Calculates the great-circle distance between two points on Earth using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point in decimal degrees.</param>
    /// <param name="lon1">Longitude of the first point in decimal degrees.</param>
    /// <param name="lat2">Latitude of the second point in decimal degrees.</param>
    /// <param name="lon2">Longitude of the second point in decimal degrees.</param>
    /// <returns>The distance between the two points in kilometers.</returns>
    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = _geographyOptions.EarthRadiusKilometers; // Earth's radius in kilometers from configuration
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    private static double ToRadians(double degrees) => degrees * Math.PI / 180;


} 