using System.Threading.Tasks;
using NetAstroBookings.Models;
using System.Collections.Generic;

namespace NetAstroBookings.Persistence
{
  /// <summary>
  /// Repository contract for persisting and querying <see cref="Flight"/> instances.
  /// </summary>
  public interface IFlightRepository
  {
    /// <summary>
    /// Adds a flight to the repository and assigns an identifier.
    /// </summary>
    /// <param name="flight">Flight to persist (without an Id).</param>
    /// <returns>The persisted flight with its Id assigned.</returns>
    Task<Flight> AddAsync(Flight flight);

    /// <summary>
    /// Returns all flights currently stored in the repository.
    /// </summary>
    /// <returns>A snapshot list of all flights.</returns>
    Task<IReadOnlyList<Flight>> ListAsync();

    /// <summary>
    /// Retrieves a flight by its identifier.
    /// </summary>
    /// <param name="id">Flight identifier.</param>
    /// <returns>The flight if found; otherwise <c>null</c>.</returns>
    Task<Flight?> GetByIdAsync(string id);
  }
}
