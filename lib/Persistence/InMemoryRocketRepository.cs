using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetAstroBookings.Models;

namespace NetAstroBookings.Persistence
{
  /// <summary>
  /// In-memory repository that assigns deterministic sequential IDs and supports concurrent requests.
  /// </summary>
  public class InMemoryRocketRepository : IRocketRepository
  {
    private readonly ConcurrentDictionary<string, Rocket> _store = new();
    private long _nextId;

    /// <summary>
    /// Adds a rocket to the in-memory store and assigns an identifier.
    /// </summary>
    /// <param name="rocket">Rocket instance without an Id.</param>
    /// <returns>The persisted rocket with <see cref="Rocket.Id"/> assigned.</returns>
    public Task<Rocket> AddAsync(Rocket rocket)
    {
      var id = "r" + Interlocked.Increment(ref _nextId).ToString("D4");
      var persisted = Clone(rocket);
      persisted.Id = id;

      _store[id] = persisted;

      return Task.FromResult(Clone(persisted));
    }

    /// <summary>
    /// Returns a snapshot list of all rockets.
    /// </summary>
    public Task<IReadOnlyList<Rocket>> ListAsync()
    {
      var snapshot = _store.Values
        .Select(Clone)
        .OrderBy(r => r.Id, StringComparer.Ordinal)
        .ToList();

      return Task.FromResult<IReadOnlyList<Rocket>>(snapshot);
    }

    /// <summary>
    /// Retrieves a rocket by its identifier.
    /// </summary>
    /// <param name="id">Rocket identifier.</param>
    /// <returns>The rocket if found; otherwise <c>null</c>.</returns>
    public Task<Rocket?> GetByIdAsync(string id)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return Task.FromResult<Rocket?>(null);
      }

      return Task.FromResult(_store.TryGetValue(id, out var rocket) ? Clone(rocket) : null);
    }

    private static Rocket Clone(Rocket rocket)
    {
      return new Rocket
      {
        Id = rocket.Id,
        Name = rocket.Name,
        Capacity = rocket.Capacity,
        Speed = rocket.Speed,
        Range = rocket.Range
      };
    }
  }
}
