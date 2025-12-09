using System;

namespace NetAstroBookings.Models
{
  /// <summary>
  /// Posibles rangos/misiones de un cohete.
  /// </summary>
  public enum RocketRange
  {
    LEO,
    Moon,
    Mars
  }

  /// <summary>
  /// Modelo de dominio que representa un cohete.
  /// </summary>
  public class Rocket
  {
    /// <summary>
    /// Identificador único asignado por el repositorio (por ejemplo "r0001").
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Nombre del cohete.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capacidad (número de pasajeros).
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Rango o destino del cohete.
    /// </summary>
    public RocketRange Range { get; set; }
  }
}
