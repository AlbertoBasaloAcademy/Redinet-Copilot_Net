using System;

namespace NetAstroBookings.Dtos
{
  public class RocketResponseDto
  {
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Range { get; set; } = string.Empty;
  }
}