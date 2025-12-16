using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IBookingRepository _bookingRepository;
    private readonly IRocketRepository _rocketRepository;
    private readonly IFlightOperationGate _operationGate;
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
      IBookingRepository bookingRepository,
      IRocketRepository rocketRepository,
      IFlightOperationGate operationGate,
      TimeProvider timeProvider,
      ILogger<FlightService> logger)
    {
      _flightRepository = flightRepository;
      _bookingRepository = bookingRepository;
      _rocketRepository = rocketRepository;
      _operationGate = operationGate;
      _timeProvider = timeProvider;
      _logger = logger;
    }

    /// <summary>
    /// Marks an existing flight as cancelled.
    /// This operation is idempotent; calling it multiple times will not trigger duplicate workflows.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <returns>A result describing either success, not found, a conflict, or an unexpected failure.</returns>
    public async Task<FlightOperationResult> CancelAsync(string flightId)
    {
      if (string.IsNullOrWhiteSpace(flightId))
      {
        return new FlightOperationResult.NotFound();
      }

      await using var gate = await _operationGate.AcquireAsync(flightId);

      var flight = await _flightRepository.GetByIdAsync(flightId);
      if (flight is null)
      {
        return new FlightOperationResult.NotFound();
      }

      if (flight.State == FlightState.CANCELLED)
      {
        _logger.LogInformation("Cancel requested for flight {FlightId} but it is already CANCELLED", flightId);
        return new FlightOperationResult.Success(flight);
      }

      if (flight.State == FlightState.DONE)
      {
        _logger.LogWarning(
          "Rejected cancel transition for flight {FlightId}. State={State} Reason={Reason}",
          flightId,
          flight.State,
          "cannot cancel a performed flight");

        return new FlightOperationResult.Conflict("flight cannot be cancelled because it is already DONE");
      }

      var fromState = flight.State;
      flight.State = FlightState.CANCELLED;

      var updated = await _flightRepository.UpdateAsync(flight);
      if (updated is null)
      {
        _logger.LogError("Failed to cancel flight {FlightId}", flightId);
        return new FlightOperationResult.UnexpectedFailure("failed to cancel flight");
      }

      var bookingCount = await _bookingRepository.CountByFlightIdAsync(flightId);
      var rocket = await _rocketRepository.GetByIdAsync(updated.RocketId);
      var capacity = rocket?.Capacity ?? -1;

      _logger.LogInformation(
        "Flight {FlightId} transitioned from {FromState} to {ToState}. BookingCount={BookingCount} MinimumPassengers={MinimumPassengers} Capacity={Capacity}",
        flightId,
        fromState,
        updated.State,
        bookingCount,
        updated.MinimumPassengers,
        capacity);

      _logger.LogInformation(
        "Triggering cancellation notification/refund workflow for flight {FlightId}. BookingCount={BookingCount}",
        flightId,
        bookingCount);

      return new FlightOperationResult.Success(updated);
    }

    /// <summary>
    /// Marks an existing flight as performed (done).
    /// This operation is idempotent.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <returns>A result describing either success, not found, a conflict, or an unexpected failure.</returns>
    public async Task<FlightOperationResult> PerformAsync(string flightId)
    {
      if (string.IsNullOrWhiteSpace(flightId))
      {
        return new FlightOperationResult.NotFound();
      }

      await using var gate = await _operationGate.AcquireAsync(flightId);

      var flight = await _flightRepository.GetByIdAsync(flightId);
      if (flight is null)
      {
        return new FlightOperationResult.NotFound();
      }

      if (flight.State == FlightState.DONE)
      {
        _logger.LogInformation("Perform requested for flight {FlightId} but it is already DONE", flightId);
        return new FlightOperationResult.Success(flight);
      }

      if (flight.State == FlightState.CANCELLED)
      {
        _logger.LogWarning(
          "Rejected perform transition for flight {FlightId}. State={State} Reason={Reason}",
          flightId,
          flight.State,
          "cannot perform a cancelled flight");

        return new FlightOperationResult.Conflict("flight cannot be performed because it is CANCELLED");
      }

      var fromState = flight.State;
      flight.State = FlightState.DONE;

      var updated = await _flightRepository.UpdateAsync(flight);
      if (updated is null)
      {
        _logger.LogError("Failed to mark flight {FlightId} as DONE", flightId);
        return new FlightOperationResult.UnexpectedFailure("failed to mark flight as DONE");
      }

      var bookingCount = await _bookingRepository.CountByFlightIdAsync(flightId);
      var rocket = await _rocketRepository.GetByIdAsync(updated.RocketId);
      var capacity = rocket?.Capacity ?? -1;

      _logger.LogInformation(
        "Flight {FlightId} transitioned from {FromState} to {ToState}. BookingCount={BookingCount} MinimumPassengers={MinimumPassengers} Capacity={Capacity}",
        flightId,
        fromState,
        updated.State,
        bookingCount,
        updated.MinimumPassengers,
        capacity);

      return new FlightOperationResult.Success(updated);
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
    /// Lists only flights whose <see cref="Flight.LaunchDate"/> is strictly greater than the current time,
    /// and optionally filters by flight state.
    /// </summary>
    /// <param name="state">Optional state filter (case-insensitive).</param>
    /// <returns>A result describing either success or a validation failure.</returns>
    public async Task<ListFlightsResult> ListFutureFlightsAsync(string? state)
    {
      FlightState? parsedState = null;

      if (state is not null)
      {
        if (string.IsNullOrWhiteSpace(state))
        {
          const string error = "state must be a valid flight state";
          _logger.LogWarning("Invalid flight state filter value: {State}", state);
          return new ListFlightsResult.ValidationFailed(error);
        }

        var trimmed = state.Trim();
        if (!Enum.TryParse<FlightState>(trimmed, true, out var parsed))
        {
          const string error = "state must be one of: SCHEDULED, CONFIRMED, SOLD_OUT, CANCELLED, DONE";
          _logger.LogWarning("Invalid flight state filter value: {State}", trimmed);
          return new ListFlightsResult.ValidationFailed(error);
        }

        parsedState = parsed;
      }

      var now = _timeProvider.GetUtcNow();

      var allFlights = await _flightRepository.ListAsync();
      var futureFlights = allFlights
        .Where(f => f.LaunchDate > now)
        .Where(f => parsedState is null || f.State == parsedState.Value)
        .OrderBy(f => f.LaunchDate)
        .ThenBy(f => f.Id, StringComparer.Ordinal)
        .ToList();

      _logger.LogInformation(
        "Listed future flights. Count={Count} StateFilter={StateFilter}",
        futureFlights.Count,
        parsedState?.ToString() ?? "(none)");

      return new ListFlightsResult.Success(futureFlights);
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

    /// <summary>
    /// Result type for list operations.
    /// </summary>
    public abstract record ListFlightsResult
    {
      /// <summary>
      /// Validation failure outcome.
      /// </summary>
      public sealed record ValidationFailed(string Error) : ListFlightsResult;

      /// <summary>
      /// Successful list outcome.
      /// </summary>
      public sealed record Success(IReadOnlyList<Flight> Flights) : ListFlightsResult;
    }

    /// <summary>
    /// Result type for operational flight transitions.
    /// </summary>
    public abstract record FlightOperationResult
    {
      /// <summary>
      /// Successful transition (or idempotent no-op) outcome.
      /// </summary>
      public sealed record Success(Flight Flight) : FlightOperationResult;

      /// <summary>
      /// Outcome when the referenced flight does not exist.
      /// </summary>
      public sealed record NotFound : FlightOperationResult;

      /// <summary>
      /// Conflict outcome when the requested transition is invalid.
      /// </summary>
      public sealed record Conflict(string Error) : FlightOperationResult;

      /// <summary>
      /// Unexpected failure outcome.
      /// </summary>
      public sealed record UnexpectedFailure(string Error) : FlightOperationResult;
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
