using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// DTO de respuesta devuelto por el API para representar un cohete persistido.
  /// </summary>
  public class RocketResponseDto
  {
    /// <summary>
    /// Identificador asignado por el repositorio.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Nombre del cohete.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capacidad del cohete.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Rango del cohete como cadena (por ejemplo "LEO", "Moon", "Mars").
    /// </summary>
    public string Range { get; set; } = string.Empty;
  }
}