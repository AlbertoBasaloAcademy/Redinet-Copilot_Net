using System;

namespace NetAstroBookings.Models
{
  /// <summary>
  /// Supported mission ranges for a rocket.
  /// </summary>
  public enum RocketRange
  {
    LEO,
    MOON,
    MARS
  }

  /// <summary>
  /// Domain model that represents a rocket.
  /// </summary>
  public class Rocket
  {
    /// <summary>
    /// Unique identifier assigned by the repository (for example "r0001").
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Rocket name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capacity (number of passengers).
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Optional speed for the rocket.
    /// </summary>
    public int? Speed { get; set; }

    /// <summary>
    /// Rocket mission range.
    /// </summary>
    public RocketRange Range { get; set; }
  }
}
