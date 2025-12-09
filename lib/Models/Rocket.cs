using System;

namespace NetAstroBookings.Models
{
  public enum RocketRange
  {
    LEO,
    Moon,
    Mars
  }

  public class Rocket
  {
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RocketRange Range { get; set; }
  }
}
