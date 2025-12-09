using System;

namespace NetAstroBookings.Dtos
{
  /// <summary>
  /// Data transfer object usado para crear un cohete.
  /// </summary>
  public class RocketDto
  {
    /// <summary>
    /// Nombre del cohete. Requerido.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capacidad (número de pasajeros). Debe ser mayor que 0 y menor o igual a 10.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Rango del cohete expresado como cadena que coincide con <see cref="NetAstroBookings.Models.RocketRange"/>.
    /// Valor por defecto: "LEO".
    /// </summary>
    public string Range { get; set; } = "LEO"; // accept enum names as string
  }
}
