using FareCalculator.Configuration;
using FareCalculator.Interfaces;
using FareCalculator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FareCalculator.Services;

/// <summary>
/// Provides metro line management services including line lookup, route calculation, and transfer analysis.
/// </summary>
public class MetroLineService : IMetroLineService
{
    private readonly ILogger<MetroLineService> _logger;
    private readonly List<MetroLine> _metroLines;
    private readonly IStationService _stationService;
    private readonly MetroLineOptions _metroLineOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetroLineService"/> class.
    /// </summary>
    /// <param name="logger">The logger for capturing service execution information.</param>
    /// <param name="metroLineOptions">Configuration options for metro line data.</param>
    /// <param name="stationService">The station service for station operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public MetroLineService(
        ILogger<MetroLineService> logger,
        IOptions<MetroLineOptions> metroLineOptions,
        IStationService stationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var optionsValue = metroLineOptions?.Value ?? throw new ArgumentNullException(nameof(metroLineOptions));
        _metroLineOptions = optionsValue;
        _metroLines = optionsValue.Lines ?? throw new ArgumentNullException(nameof(metroLineOptions));
        _stationService = stationService ?? throw new ArgumentNullException(nameof(stationService));
    }

    /// <summary>
    /// Retrieves a metro line by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the metro line.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the metro line if found, otherwise null.</returns>
    public Task<MetroLine?> GetMetroLineByIdAsync(int id)
    {
        _logger.LogInformation("Getting metro line by ID: {Id}", id);
        var metroLine = _metroLines.FirstOrDefault(ml => ml.Id == id);
        return Task.FromResult(metroLine);
    }

    /// <summary>
    /// Retrieves a metro line by its code asynchronously using case-insensitive search.
    /// </summary>
    /// <param name="code">The code of the metro line to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the metro line if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when the code parameter is null or empty.</exception>
    public Task<MetroLine?> GetMetroLineByCodeAsync(string code)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Metro line code cannot be null or empty.", nameof(code));

        _logger.LogInformation("Getting metro line by code: {Code}", code);
        var metroLine = _metroLines.FirstOrDefault(ml => 
            ml.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(metroLine);
    }

    /// <summary>
    /// Retrieves all available metro lines in the system asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all metro lines.</returns>
    public Task<IEnumerable<MetroLine>> GetAllMetroLinesAsync()
    {
        _logger.LogInformation("Getting all metro lines");
        return Task.FromResult<IEnumerable<MetroLine>>(_metroLines.Where(ml => ml.IsOperational));
    }

    /// <summary>
    /// Gets all metro lines that serve a specific station asynchronously.
    /// </summary>
    /// <param name="station">The station to check for metro lines.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains metro lines serving the station.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the station parameter is null.</exception>
    public Task<IEnumerable<MetroLine>> GetMetroLinesByStationAsync(Station station)
    {
        if (station == null)
            throw new ArgumentNullException(nameof(station));

        _logger.LogInformation("Getting metro lines for station: {StationName}", station.Name);
        var metroLines = _metroLines.Where(ml => 
            ml.IsOperational && station.MetroLineIds.Contains(ml.Id));
        return Task.FromResult(metroLines);
    }

    /// <summary>
    /// Gets all stations served by a specific metro line asynchronously.
    /// </summary>
    /// <param name="metroLine">The metro line to get stations for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains stations served by the metro line.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the metroLine parameter is null.</exception>
    public async Task<IEnumerable<Station>> GetStationsByMetroLineAsync(MetroLine metroLine)
    {
        if (metroLine == null)
            throw new ArgumentNullException(nameof(metroLine));

        _logger.LogInformation("Getting stations for metro line: {LineCode}", metroLine.Code);
        var allStations = await _stationService.GetAllStationsAsync();
        return allStations.Where(station => station.MetroLineIds.Contains(metroLine.Id));
    }

    /// <summary>
    /// Determines if a direct route exists between two stations on the same metro line.
    /// </summary>
    /// <param name="origin">The origin station.</param>
    /// <param name="destination">The destination station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates if a direct route exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    public Task<bool> HasDirectRouteAsync(Station origin, Station destination)
    {
        if (origin == null)
            throw new ArgumentNullException(nameof(origin));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        _logger.LogInformation("Checking direct route between {Origin} and {Destination}", 
            origin.Name, destination.Name);

        // Check if there's a common metro line between the two stations
        var hasDirectRoute = origin.MetroLineIds.Intersect(destination.MetroLineIds).Any();
        return Task.FromResult(hasDirectRoute);
    }

    /// <summary>
    /// Calculates the required transfers between two stations and returns the optimal route.
    /// </summary>
    /// <param name="origin">The origin station.</param>
    /// <param name="destination">The destination station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the optimal route with transfer information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    public async Task<MetroRoute> CalculateOptimalRouteAsync(Station origin, Station destination)
    {
        if (origin == null)
            throw new ArgumentNullException(nameof(origin));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        _logger.LogInformation("Calculating optimal route between {Origin} and {Destination}", 
            origin.Name, destination.Name);

        var route = new MetroRoute
        {
            Origin = origin,
            Destination = destination
        };

        // Check for direct route first
        var commonLines = origin.MetroLineIds.Intersect(destination.MetroLineIds).ToList();
        if (commonLines.Any())
        {
            // Direct route available
            var directLine = await GetMetroLineByIdAsync(commonLines.First());
            if (directLine != null)
            {
                var distance = await _stationService.CalculateDistanceAsync(origin, destination);
                route.Segments.Add(new RouteSegment
                {
                    MetroLine = directLine,
                    StartStation = origin,
                    EndStation = destination,
                    DistanceKilometers = distance,
                    EstimatedTimeMinutes = (int)(distance * 2) // Estimate 2 minutes per km
                });
                route.TotalDistanceKilometers = distance;
                route.EstimatedTravelTimeMinutes = route.Segments.Sum(s => s.EstimatedTimeMinutes);
            }
        }
        else
        {
            // Need to find route with transfers
            var transferRoute = await FindRouteWithTransfersAsync(origin, destination);
            if (transferRoute != null)
            {
                route = transferRoute;
            }
        }

        return route;
    }

    /// <summary>
    /// Finds a route with transfers between two stations.
    /// </summary>
    /// <param name="origin">The origin station.</param>
    /// <param name="destination">The destination station.</param>
    /// <returns>A metro route with transfer information, or null if no route is found.</returns>
    private async Task<MetroRoute?> FindRouteWithTransfersAsync(Station origin, Station destination)
    {
        var allStations = (await _stationService.GetAllStationsAsync()).ToList();
        
        // Find transfer stations that connect the origin and destination lines
        var transferStations = allStations.Where(s => 
            s.IsTransferStation && 
            s.MetroLineIds.Intersect(origin.MetroLineIds).Any() &&
            s.MetroLineIds.Intersect(destination.MetroLineIds).Any()).ToList();

        if (!transferStations.Any())
            return null;

        // Use the first viable transfer station for simplicity
        // In a real-world scenario, you'd evaluate all options for the optimal route
        var transferStation = transferStations.First();
        
        var route = new MetroRoute
        {
            Origin = origin,
            Destination = destination,
            TransferCount = 1,
            TransferStations = { transferStation }
        };

        // First segment: origin to transfer station
        var originLine = await GetMetroLineByIdAsync(origin.MetroLineIds.Intersect(transferStation.MetroLineIds).First());
        var firstDistance = await _stationService.CalculateDistanceAsync(origin, transferStation);
        
        if (originLine != null)
        {
            route.Segments.Add(new RouteSegment
            {
                MetroLine = originLine,
                StartStation = origin,
                EndStation = transferStation,
                DistanceKilometers = firstDistance,
                EstimatedTimeMinutes = (int)(firstDistance * 2)
            });
        }

        // Second segment: transfer station to destination
        var destinationLine = await GetMetroLineByIdAsync(destination.MetroLineIds.Intersect(transferStation.MetroLineIds).First());
        var secondDistance = await _stationService.CalculateDistanceAsync(transferStation, destination);
        
        if (destinationLine != null)
        {
            route.Segments.Add(new RouteSegment
            {
                MetroLine = destinationLine,
                StartStation = transferStation,
                EndStation = destination,
                DistanceKilometers = secondDistance,
                EstimatedTimeMinutes = (int)(secondDistance * 2)
            });
        }

        // Add transfer time (assume 5 minutes for transfers)
        route.TotalDistanceKilometers = route.Segments.Sum(s => s.DistanceKilometers);
        route.EstimatedTravelTimeMinutes = route.Segments.Sum(s => s.EstimatedTimeMinutes) + (route.TransferCount * 5);

        return route;
    }
} 