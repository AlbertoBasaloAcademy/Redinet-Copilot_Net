using System;
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
  public class InMemoryFlightRepository : IFlightRepository
  {
    private readonly ConcurrentDictionary<string, Flight> _store = new();
    private long _nextId;

    /// <summary>
    /// Adds a flight to the in-memory store and assigns an identifier.
    /// </summary>
    /// <param name="flight">Flight instance without an Id.</param>
    /// <returns>The persisted flight with <see cref="Flight.Id"/> assigned.</returns>
    public Task<Flight> AddAsync(Flight flight)
    {
      var id = "f" + Interlocked.Increment(ref _nextId).ToString("D4");
      var persisted = Clone(flight);
      persisted.Id = id;

      _store[id] = persisted;

      return Task.FromResult(Clone(persisted));
    }

    /// <summary>
    /// Returns a snapshot list of all flights.
    /// </summary>
    public Task<IReadOnlyList<Flight>> ListAsync()
    {
      var snapshot = _store.Values
        .Select(Clone)
        .OrderBy(f => f.LaunchDate)
        .ThenBy(f => f.Id, StringComparer.Ordinal)
        .ToList();

      return Task.FromResult<IReadOnlyList<Flight>>(snapshot);
    }

    /// <summary>
    /// Retrieves a flight by its identifier.
    /// </summary>
    /// <param name="id">Flight identifier.</param>
    /// <returns>The flight if found; otherwise <c>null</c>.</returns>
    public Task<Flight?> GetByIdAsync(string id)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return Task.FromResult<Flight?>(null);
      }

      return Task.FromResult(_store.TryGetValue(id, out var flight) ? Clone(flight) : null);
    }

    /// <summary>
    /// Updates an existing flight.
    /// </summary>
    /// <param name="flight">Flight instance with an existing <see cref="Flight.Id"/>.</param>
    /// <returns>The updated flight if found; otherwise <c>null</c>.</returns>
    public Task<Flight?> UpdateAsync(Flight flight)
    {
      if (flight is null)
      {
        return Task.FromResult<Flight?>(null);
      }

      var id = flight.Id;
      if (string.IsNullOrWhiteSpace(id))
      {
        return Task.FromResult<Flight?>(null);
      }

      if (!_store.ContainsKey(id))
      {
        return Task.FromResult<Flight?>(null);
      }

      var persisted = Clone(flight);
      persisted.Id = id;

      _store[id] = persisted;

      return Task.FromResult<Flight?>(Clone(persisted));
    }

    private static Flight Clone(Flight flight)
    {
      return new Flight
      {
        Id = flight.Id,
        RocketId = flight.RocketId,
        LaunchDate = flight.LaunchDate,
        BasePrice = flight.BasePrice,
        MinimumPassengers = flight.MinimumPassengers,
        State = flight.State
      };
    }
  }
}
