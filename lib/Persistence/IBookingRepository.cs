using System.Collections.Generic;
using System.Threading.Tasks;
using NetAstroBookings.Models;

namespace NetAstroBookings.Persistence
{
  /// <summary>
  /// Repository contract for persisting and querying <see cref="Booking"/> instances.
  /// </summary>
  public interface IBookingRepository
  {
    /// <summary>
    /// Adds a booking to the repository and assigns an identifier.
    /// </summary>
    /// <param name="booking">Booking to persist (without an Id).</param>
    /// <returns>The persisted booking with its Id assigned.</returns>
    Task<Booking> AddAsync(Booking booking);

    /// <summary>
    /// Returns the number of bookings currently stored for a given flight.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <returns>Number of bookings for the flight.</returns>
    Task<int> CountByFlightIdAsync(string flightId);

    /// <summary>
    /// Returns a snapshot list of all bookings for a given flight.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <returns>A snapshot list of bookings for the flight.</returns>
    Task<IReadOnlyList<Booking>> ListByFlightIdAsync(string flightId);
  }
}
