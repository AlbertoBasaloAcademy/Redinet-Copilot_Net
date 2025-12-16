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
  public class InMemoryBookingRepository : IBookingRepository
  {
    private readonly ConcurrentDictionary<string, Booking> _store = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _byFlightId = new(StringComparer.Ordinal);
    private long _nextId;

    /// <summary>
    /// Adds a booking to the in-memory store and assigns an identifier.
    /// </summary>
    /// <param name="booking">Booking instance without an Id.</param>
    /// <returns>The persisted booking with <see cref="Booking.Id"/> assigned.</returns>
    public Task<Booking> AddAsync(Booking booking)
    {
      var id = "b" + Interlocked.Increment(ref _nextId).ToString("D4");
      var persisted = Clone(booking);
      persisted.Id = id;

      _store[id] = persisted;

      var index = _byFlightId.GetOrAdd(persisted.FlightId, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
      index[id] = 0;

      return Task.FromResult(Clone(persisted));
    }

    /// <summary>
    /// Returns the number of bookings currently stored for a given flight.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <returns>Number of bookings for the flight.</returns>
    public Task<int> CountByFlightIdAsync(string flightId)
    {
      if (string.IsNullOrWhiteSpace(flightId))
      {
        return Task.FromResult(0);
      }

      return Task.FromResult(_byFlightId.TryGetValue(flightId, out var index) ? index.Count : 0);
    }

    /// <summary>
    /// Returns a snapshot list of all bookings for a given flight.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <returns>A snapshot list of bookings for the flight.</returns>
    public Task<IReadOnlyList<Booking>> ListByFlightIdAsync(string flightId)
    {
      if (string.IsNullOrWhiteSpace(flightId))
      {
        return Task.FromResult<IReadOnlyList<Booking>>(Array.Empty<Booking>());
      }

      if (!_byFlightId.TryGetValue(flightId, out var index))
      {
        return Task.FromResult<IReadOnlyList<Booking>>(Array.Empty<Booking>());
      }

      var snapshot = index.Keys
        .Select(id => _store.TryGetValue(id, out var booking) ? Clone(booking) : null)
        .Where(b => b is not null)
        .Select(b => b!)
        .OrderBy(b => b.Id, StringComparer.Ordinal)
        .ToList();

      return Task.FromResult<IReadOnlyList<Booking>>(snapshot);
    }

    private static Booking Clone(Booking booking)
    {
      return new Booking
      {
        Id = booking.Id,
        FlightId = booking.FlightId,
        PassengerName = booking.PassengerName,
        PassengerEmail = booking.PassengerEmail
      };
    }
  }
}
