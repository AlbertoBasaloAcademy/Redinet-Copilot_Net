using System;

namespace NetAstroBookings.Models
{
  /// <summary>
  /// Represents the lifecycle state of a flight.
  /// </summary>
  public enum FlightState
  {
    SCHEDULED,
    CONFIRMED,
    SOLD_OUT,
    CANCELLED,
    DONE
  }
}
