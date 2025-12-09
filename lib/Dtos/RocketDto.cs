using System;

namespace NetAstroBookings.Dtos
{
  public class RocketDto
  {
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Range { get; set; } = "LEO"; // accept enum names as string
  }
}
