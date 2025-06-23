using FareCalculator.Models;

namespace FareCalculator.Interfaces;

/// <summary>
/// Provides services for managing and retrieving station information and calculating distances between stations.
/// </summary>
public interface IStationService
{
    /// <summary>
    /// Retrieves a station by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the station if found, otherwise null.</returns>
    Task<Station?> GetStationByIdAsync(int id);

    /// <summary>
    /// Retrieves a station by its name asynchronously (case-insensitive search).
    /// </summary>
    /// <param name="name">The name of the station to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the station if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when the name parameter is null or empty.</exception>
    Task<Station?> GetStationByNameAsync(string name);

    /// <summary>
    /// Retrieves all available stations in the metro system asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all stations.</returns>
    Task<IEnumerable<Station>> GetAllStationsAsync();

    /// <summary>
    /// Calculates the distance between two stations in kilometers using their geographical coordinates.
    /// </summary>
    /// <param name="origin">The origin station.</param>
    /// <param name="destination">The destination station.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the distance in kilometers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either origin or destination parameter is null.</exception>
    Task<double> CalculateDistanceAsync(Station origin, Station destination);
} 