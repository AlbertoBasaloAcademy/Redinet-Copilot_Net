using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// Response DTO returned by the API to represent a persisted rocket.
  /// </summary>
  public class RocketResponseDto
  {
    /// <summary>
    /// Identifier assigned by the repository.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Rocket name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Rocket capacity.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Optional rocket speed.
    /// </summary>
    public int? Speed { get; set; }

    /// <summary>
    /// Rocket range as a string (for example "LEO", "MOON", "MARS").
    /// </summary>
    public string Range { get; set; } = string.Empty;
  }
}