using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetAstroBookings.Dtos;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;

namespace NetAstroBookings.Business
{
  /// <summary>
  /// Business service for operations related to <see cref="Rocket"/>.
  /// Encapsulates validation rules and orchestrates persistence via a repository abstraction.
  /// </summary>
  public class RocketService
  {
    private readonly IRocketRepository _repository;
    private readonly ILogger<RocketService> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="RocketService"/>.
    /// </summary>
    /// <param name="repository">Repository used to persist and query rockets.</param>
    /// <param name="logger">Logger instance.</param>
    public RocketService(IRocketRepository repository, ILogger<RocketService> logger)
    {
      _repository = repository;
      _logger = logger;
    }

    /// <summary>
    /// Validates input data and creates a new <see cref="Rocket"/>.
    /// </summary>
    /// <param name="dto">DTO con los datos del cohete a crear.</param>
    /// <returns>A result describing either success or a validation failure.</returns>
    public async Task<CreateRocketResult> CreateAsync(RocketDto dto)
    {
      var validation = ValidateDto(dto);
      if (validation is CreateRocketResult.ValidationFailed validationFailed)
      {
        _logger.LogWarning("Rocket validation failed: {Error}", validationFailed.Error);
        return validationFailed;
      }

      var range = ((CreateRocketResult.Validated)validation).Range;

      var rocket = new Rocket
      {
        Name = dto.Name.Trim(),
        Capacity = dto.Capacity,
        Speed = dto.Speed,
        Range = range
      };

      var created = await _repository.AddAsync(rocket);
      _logger.LogInformation("Created rocket {RocketId}", created.Id);
      return new CreateRocketResult.Success(created);
    }

    /// <summary>
    /// Returns all rockets.
    /// </summary>
    /// <returns>A snapshot list of all rockets.</returns>
    public Task<IReadOnlyList<Rocket>> ListAsync()
    {
      return _repository.ListAsync();
    }

    /// <summary>
    /// Retrieves a rocket by its identifier.
    /// </summary>
    /// <param name="id">Rocket identifier.</param>
    /// <returns>A result describing either a found rocket or not found.</returns>
    public async Task<GetRocketResult> GetByIdAsync(string id)
    {
      var rocket = await _repository.GetByIdAsync(id);
      return rocket is null ? new GetRocketResult.NotFound() : new GetRocketResult.Found(rocket);
    }

    /// <summary>
    /// Result type for create operations.
    /// </summary>
    public abstract record CreateRocketResult
    {
      /// <summary>
      /// Internal representation for validated input.
      /// </summary>
      public sealed record Validated(RocketRange Range) : CreateRocketResult;

      /// <summary>
      /// Validation failure outcome.
      /// </summary>
      public sealed record ValidationFailed(string Error) : CreateRocketResult;

      /// <summary>
      /// Successful creation outcome.
      /// </summary>
      public sealed record Success(Rocket Rocket) : CreateRocketResult;
    }

    /// <summary>
    /// Result type for get-by-id operations.
    /// </summary>
    public abstract record GetRocketResult
    {
      /// <summary>
      /// Rocket not found.
      /// </summary>
      public sealed record NotFound : GetRocketResult;

      /// <summary>
      /// Rocket found.
      /// </summary>
      public sealed record Found(Rocket Rocket) : GetRocketResult;
    }

    private CreateRocketResult ValidateDto(RocketDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.Name))
      {
        return new CreateRocketResult.ValidationFailed("Name is required");
      }

      if (dto.Capacity <= 0 || dto.Capacity > 10)
      {
        return new CreateRocketResult.ValidationFailed("Capacity must be > 0 and <= 10");
      }

      var rangeText = dto.Range;
      if (string.IsNullOrWhiteSpace(rangeText))
      {
        rangeText = "LEO";
      }

      if (!Enum.TryParse<RocketRange>(rangeText, true, out var range))
      {
        return new CreateRocketResult.ValidationFailed("Range must be one of: LEO, MOON, MARS");
      }

      return new CreateRocketResult.Validated(range);
    }
  }
}
