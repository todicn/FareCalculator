using FareCalculator.Models;

namespace FareCalculator.Interfaces;

/// <summary>
/// Provides services for managing and retrieving metro line information and operations.
/// </summary>
public interface IMetroLineService
{
    /// <summary>
    /// Retrieves a metro line by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the metro line.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the metro line if found, otherwise null.</returns>
    Task<MetroLine?> GetMetroLineByIdAsync(int id);

    /// <summary>
    /// Retrieves a metro line by its code asynchronously (case-insensitive search).
    /// </summary>
    /// <param name="code">The code of the metro line to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the metro line if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when the code parameter is null or empty.</exception>
    Task<MetroLine?> GetMetroLineByCodeAsync(string code);

    /// <summary>
    /// Retrieves all available metro lines in the system asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all metro lines.</returns>
    Task<IEnumerable<MetroLine>> GetAllMetroLinesAsync();

    /// <summary>
    /// Gets all metro lines that serve a specific station asynchronously.
    /// </summary>
    /// <param name="station">The station to check for metro lines.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains metro lines serving the station.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the station parameter is null.</exception>
    Task<IEnumerable<MetroLine>> GetMetroLinesByStationAsync(Station station);

    /// <summary>
    /// Gets all stations served by a specific metro line asynchronously.
    /// </summary>
    /// <param name="metroLine">The metro line to get stations for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains stations served by the metro line.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the metroLine parameter is null.</exception>
    Task<IEnumerable<Station>> GetStationsByMetroLineAsync(MetroLine metroLine);

    /// <summary>
    /// Determines if a direct route exists between two stations on the same metro line.
    /// </summary>
    /// <param name="origin">The origin station.</param>
    /// <param name="destination">The destination station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates if a direct route exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    Task<bool> HasDirectRouteAsync(Station origin, Station destination);

    /// <summary>
    /// Calculates the required transfers between two stations and returns the optimal route.
    /// </summary>
    /// <param name="origin">The origin station.</param>
    /// <param name="destination">The destination station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the optimal route with transfer information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    Task<MetroRoute> CalculateOptimalRouteAsync(Station origin, Station destination);
} 