using System;

namespace NetAstroBookings.Models
{
  /// <summary>
  /// Domain model that represents a scheduled flight linked to a rocket.
  /// </summary>
  public class Flight
  {
    /// <summary>
    /// Unique identifier assigned by the repository (for example "f0001").
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Identifier of the rocket associated with this flight.
    /// </summary>
    public string RocketId { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled launch date and time.
    /// Must be in the future when creating the flight.
    /// </summary>
    public DateTimeOffset LaunchDate { get; set; }

    /// <summary>
    /// Base price for bookings on this flight.
    /// Must be greater than 0 when creating the flight.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Minimum number of passengers required for the flight.
    /// Defaults to 5 when omitted on input.
    /// </summary>
    public int MinimumPassengers { get; set; }

    /// <summary>
    /// Current state of the flight.
    /// </summary>
    public FlightState State { get; set; }
  }
}
