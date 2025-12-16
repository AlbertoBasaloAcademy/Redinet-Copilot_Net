using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// Response DTO returned by the API to represent a persisted booking.
  /// </summary>
  public class BookingResponseDto
  {
    /// <summary>
    /// Identifier assigned by the repository.
    /// </summary>
    public required string Id { get; set; }

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

    /// <summary>
    /// Final computed price for the booking.
    /// </summary>
    public decimal FinalPrice { get; set; }
  }
}
