using System;
using System.Threading.Tasks;
using NetAstroBookings.Dtos;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;

namespace NetAstroBookings.Business
{
  public class RocketService
  {
    private readonly NetAstroBookings.Persistence.InMemoryRocketRepository _repository;

    public RocketService(NetAstroBookings.Persistence.InMemoryRocketRepository repository)
    {
      _repository = repository;
    }

    public async Task<Rocket> CreateAsync(RocketDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required", nameof(dto.Name));

      if (dto.Capacity <= 0 || dto.Capacity > 10)
        throw new ArgumentException("Capacity must be > 0 and <= 10", nameof(dto.Capacity));

      if (!Enum.TryParse<RocketRange>(dto.Range, true, out var range))
        throw new ArgumentException("Range must be one of: LEO, Moon, Mars", nameof(dto.Range));

      var rocket = new Rocket
      {
        Name = dto.Name.Trim(),
        Capacity = dto.Capacity,
        Range = range
      };

      return await _repository.AddAsync(rocket);
    }
  }
}
