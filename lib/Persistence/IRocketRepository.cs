using System.Collections.Generic;
using System.Threading.Tasks;
using NetAstroBookings.Models;

namespace NetAstroBookings.Persistence
{
  /// <summary>
  /// Repository contract for persisting and querying <see cref="Rocket"/> instances.
  /// </summary>
  public interface IRocketRepository
  {
    /// <summary>
    /// Adds a rocket to the repository and assigns an identifier.
    /// </summary>
    /// <param name="rocket">Rocket to persist (without an Id).</param>
    /// <returns>The persisted rocket with its Id assigned.</returns>
    Task<Rocket> AddAsync(Rocket rocket);

    /// <summary>
    /// Returns all rockets currently stored in the repository.
    /// </summary>
    /// <returns>A snapshot list of all rockets.</returns>
    Task<IReadOnlyList<Rocket>> ListAsync();

    /// <summary>
    /// Retrieves a rocket by its identifier.
    /// </summary>
    /// <param name="id">Rocket identifier.</param>
    /// <returns>The rocket if found; otherwise <c>null</c>.</returns>
    Task<Rocket?> GetByIdAsync(string id);
  }
}
