using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// Data transfer object used to create a booking for a flight.
  /// </summary>
  public class CreateBookingDto
  {
    /// <summary>
    /// Passenger name. Required.
    /// </summary>
    public string PassengerName { get; set; } = string.Empty;

    /// <summary>
    /// Passenger email. Required.
    /// </summary>
    public string PassengerEmail { get; set; } = string.Empty;
  }
}
