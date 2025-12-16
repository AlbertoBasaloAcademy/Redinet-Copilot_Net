using System;

namespace NetAstroBookings.Models
{
  /// <summary>
  /// Domain model that represents a booking for a specific flight.
  /// A booking reserves one seat for a passenger.
  /// </summary>
  public class Booking
  {
    /// <summary>
    /// Unique identifier assigned by the repository (for example "b0001").
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Identifier of the flight this booking belongs to.
    /// </summary>
    public string FlightId { get; set; } = string.Empty;

    /// <summary>
    /// Passenger name.
    /// </summary>
    public string PassengerName { get; set; } = string.Empty;

    /// <summary>
    /// Passenger email.
    /// </summary>
    public string PassengerEmail { get; set; } = string.Empty;
  }
}
