using System;
using System.Threading.Tasks;
using NetAstroBookings.Dtos;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;

namespace NetAstroBookings.Business
{
  /// <summary>
  /// Servicio de negocio para operaciones relacionadas con <see cref="NetAstroBookings.Models.Rocket"/>.
  /// Encapsula la lógica de validación y delega la persistencia al repositorio en memoria.
  /// </summary>
  public class RocketService
  {
    private readonly NetAstroBookings.Persistence.InMemoryRocketRepository _repository;

    /// <summary>
    /// Crea una nueva instancia de <see cref="RocketService"/>.
    /// </summary>
    /// <param name="repository">Repositorio en memoria para persistir cohetes.</param>
    public RocketService(NetAstroBookings.Persistence.InMemoryRocketRepository repository)
    {
      _repository = repository;
    }

    /// <summary>
    /// Valida los datos de entrada y crea un nuevo <see cref="NetAstroBookings.Models.Rocket"/>.
    /// Lanza <see cref="ArgumentException"/> si los datos son inválidos.
    /// </summary>
    /// <param name="dto">DTO con los datos del cohete a crear.</param>
    /// <returns>El cohete creado con su Id asignado.</returns>
    public async Task<Rocket> CreateAsync(RocketDto dto)
    {
      var range = ValidateDto(dto);

      var rocket = new Rocket
      {
        Name = dto.Name.Trim(),
        Capacity = dto.Capacity,
        Range = range
      };

      return await _repository.AddAsync(rocket);
    }

    private RocketRange ValidateDto(RocketDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required", nameof(dto.Name));

      if (dto.Capacity <= 0 || dto.Capacity > 10)
        throw new ArgumentException("Capacity must be > 0 and <= 10", nameof(dto.Capacity));

      if (!Enum.TryParse<RocketRange>(dto.Range, true, out var range))
        throw new ArgumentException("Range must be one of: LEO, Moon, Mars", nameof(dto.Range));

      return range;
    }
  }
}
