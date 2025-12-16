using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetAstroBookings.Dtos;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;

namespace NetAstroBookings.Business
{
  /// <summary>
  /// Business service for operations related to <see cref="Flight"/>.
  /// Encapsulates validation rules and orchestrates persistence via repository abstractions.
  /// </summary>
  public class FlightService
  {
    private const int DefaultMinimumPassengers = 5;

    private readonly IFlightRepository _flightRepository;
    private readonly IRocketRepository _rocketRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<FlightService> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="FlightService"/>.
    /// </summary>
    /// <param name="flightRepository">Repository used to persist and query flights.</param>
    /// <param name="rocketRepository">Repository used to verify referenced rockets exist.</param>
    /// <param name="timeProvider">Time provider used for deterministic time-based validation.</param>
    /// <param name="logger">Logger instance.</param>
    public FlightService(
      IFlightRepository flightRepository,
      IRocketRepository rocketRepository,
      TimeProvider timeProvider,
      ILogger<FlightService> logger)
    {
      _flightRepository = flightRepository;
      _rocketRepository = rocketRepository;
      _timeProvider = timeProvider;
      _logger = logger;
    }

    /// <summary>
    /// Validates input data and creates a new <see cref="Flight"/> linked to an existing rocket.
    /// </summary>
    /// <param name="dto">DTO with the flight data to create.</param>
    /// <returns>A result describing either success, a validation failure, or missing rocket.</returns>
    public async Task<CreateFlightResult> CreateAsync(CreateFlightDto dto)
    {
      var validation = ValidateDto(dto);
      if (validation is CreateFlightResult.ValidationFailed validationFailed)
      {
        _logger.LogWarning("Flight validation failed: {Error}", validationFailed.Error);
        return validationFailed;
      }

      var validated = (CreateFlightResult.Validated)validation;

      var rocket = await _rocketRepository.GetByIdAsync(validated.RocketId);
      if (rocket is null)
      {
        _logger.LogWarning("Rocket not found {RocketId} when creating flight", validated.RocketId);
        return new CreateFlightResult.RocketNotFound();
      }

      var flight = new Flight
      {
        RocketId = validated.RocketId,
        LaunchDate = validated.LaunchDate,
        BasePrice = validated.BasePrice,
        MinimumPassengers = validated.MinimumPassengers,
        State = FlightState.SCHEDULED
      };

      var created = await _flightRepository.AddAsync(flight);
      _logger.LogInformation("Created flight {FlightId} for rocket {RocketId}", created.Id, created.RocketId);
      return new CreateFlightResult.Success(created);
    }

    /// <summary>
    /// Result type for flight create operations.
    /// </summary>
    public abstract record CreateFlightResult
    {
      /// <summary>
      /// Internal representation for validated input.
      /// </summary>
      public sealed record Validated(string RocketId, DateTimeOffset LaunchDate, decimal BasePrice, int MinimumPassengers) : CreateFlightResult;

      /// <summary>
      /// Validation failure outcome.
      /// </summary>
      public sealed record ValidationFailed(string Error) : CreateFlightResult;

      /// <summary>
      /// Outcome when the referenced rocket does not exist.
      /// </summary>
      public sealed record RocketNotFound : CreateFlightResult;

      /// <summary>
      /// Successful creation outcome.
      /// </summary>
      public sealed record Success(Flight Flight) : CreateFlightResult;
    }

    private CreateFlightResult ValidateDto(CreateFlightDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.RocketId))
      {
        return new CreateFlightResult.ValidationFailed("rocketId is required");
      }

      var rocketId = dto.RocketId.Trim();

      var now = _timeProvider.GetUtcNow();
      if (dto.LaunchDate <= now)
      {
        return new CreateFlightResult.ValidationFailed("launchDate must be in the future");
      }

      if (dto.BasePrice <= 0m)
      {
        return new CreateFlightResult.ValidationFailed("basePrice must be > 0");
      }

      var minimumPassengers = dto.MinimumPassengers ?? DefaultMinimumPassengers;
      if (minimumPassengers <= 0)
      {
        return new CreateFlightResult.ValidationFailed("minimumPassengers must be > 0");
      }

      return new CreateFlightResult.Validated(rocketId, dto.LaunchDate, dto.BasePrice, minimumPassengers);
    }
  }
}
