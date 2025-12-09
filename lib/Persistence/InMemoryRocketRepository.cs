using System.Threading.Tasks;
using System.Collections.Concurrent;
using NetAstroBookings.Models;
using System;

namespace NetAstroBookings.Persistence
{
  /// <summary>
  /// Repositorio simple en memoria que asigna un Id secuencial al añadir cohetes.
  /// No es thread-safe para operaciones compuestas, pero usa <see cref="ConcurrentDictionary{TKey,TValue}"/>.
  /// </summary>
  public class InMemoryRocketRepository
  {
    private readonly ConcurrentDictionary<string, Rocket> _store = new();

    /// <summary>
    /// Añade un cohete al almacén en memoria y le asigna un identificador.
    /// </summary>
    /// <param name="rocket">Instancia del cohete sin Id.</param>
    /// <returns>La misma instancia con la propiedad <see cref="Rocket.Id"/> asignada.</returns>
    public Task<Rocket> AddAsync(Rocket rocket)
    {
      var count = _store.Count + 1;
      var id = "r" + count.ToString("D4");
      rocket.Id = id;
      _store[id] = rocket;
      return Task.FromResult(rocket);
    }
  }
}
