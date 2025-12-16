using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// Data transfer object used to create a flight.
  /// </summary>
  public class CreateFlightDto
  {
    /// <summary>
    /// Identifier of an existing rocket.
    /// </summary>
    public string RocketId { get; set; } = string.Empty;

    /// <summary>
    /// Launch date and time. Must be in the future.
    /// </summary>
    public DateTimeOffset LaunchDate { get; set; }

    /// <summary>
    /// Base price. Must be greater than 0.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Optional minimum number of passengers.
    /// Defaults to 5 when omitted.
    /// </summary>
    public int? MinimumPassengers { get; set; }
  }
}
