using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// Response DTO returned by the API to represent a persisted flight.
  /// </summary>
  public class FlightResponseDto
  {
    /// <summary>
    /// Identifier assigned by the repository.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Identifier of the rocket associated with this flight.
    /// </summary>
    public string RocketId { get; set; } = string.Empty;

    /// <summary>
    /// Launch date and time.
    /// </summary>
    public DateTimeOffset LaunchDate { get; set; }

    /// <summary>
    /// Base price.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Minimum number of passengers required for the flight.
    /// </summary>
    public int MinimumPassengers { get; set; }

    /// <summary>
    /// Current state of the flight.
    /// </summary>
    public string State { get; set; } = string.Empty;
  }
}
