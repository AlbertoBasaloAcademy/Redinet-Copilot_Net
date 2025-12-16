using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// Data transfer object used to create a rocket.
  /// </summary>
  public class RocketDto
  {
    /// <summary>
    /// Rocket name. Required.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capacity (number of passengers). Must be greater than 0 and less than or equal to 10.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Optional speed for the rocket.
    /// </summary>
    public int? Speed { get; set; }

    /// <summary>
    /// Mission range for the rocket as a string.
    /// Default value: "LEO".
    /// </summary>
    public string Range { get; set; } = "LEO";
  }
}
