using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetAstroBookings.Dtos;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;

namespace NetAstroBookings.Business
{
  /// <summary>
  /// Business service for operations related to <see cref="Booking"/>.
  /// Encapsulates validation rules and orchestrates persistence via repository abstractions.
  /// </summary>
  public class BookingService
  {
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IRocketRepository _rocketRepository;
    private readonly IFlightOperationGate _operationGate;
    private readonly ILogger<BookingService> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="BookingService"/>.
    /// </summary>
    /// <param name="bookingRepository">Repository used to persist and query bookings.</param>
    /// <param name="flightRepository">Repository used to load and update flights.</param>
    /// <param name="rocketRepository">Repository used to load rocket capacity.</param>
    /// <param name="logger">Logger instance.</param>
    public BookingService(
      IBookingRepository bookingRepository,
      IFlightRepository flightRepository,
      IRocketRepository rocketRepository,
      IFlightOperationGate operationGate,
      ILogger<BookingService> logger)
    {
      _bookingRepository = bookingRepository;
      _flightRepository = flightRepository;
      _rocketRepository = rocketRepository;
      _operationGate = operationGate;
      _logger = logger;
    }

    /// <summary>
    /// Validates input data and creates a booking for a specific flight.
    /// Enforces flight-state rules and capacity based on the linked rocket.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <param name="dto">DTO with passenger information.</param>
    /// <returns>A result describing either success, a validation failure, not found, or a conflict.</returns>
    public async Task<CreateBookingResult> CreateAsync(string flightId, CreateBookingDto dto)
    {
      var validation = ValidateDto(flightId, dto);
      if (validation is not CreateBookingResult.Validated validated)
      {
        if (validation is CreateBookingResult.ValidationFailed failed)
        {
          _logger.LogWarning("Booking validation failed: {Error}", failed.Error);
        }

        return validation;
      }

      await using var gate = await _operationGate.AcquireAsync(flightId);

      var flight = await _flightRepository.GetByIdAsync(flightId);
      if (flight is null)
      {
        return new CreateBookingResult.FlightNotFound();
      }

      if (flight.State is FlightState.CANCELLED or FlightState.SOLD_OUT or FlightState.DONE)
      {
        _logger.LogWarning(
          "Rejected booking for flight {FlightId} due to state {State}. Reason={Reason}",
          flightId,
          flight.State,
          "flight is not bookable");

        return new CreateBookingResult.Conflict("flight is not bookable");
      }

      var rocket = await _rocketRepository.GetByIdAsync(flight.RocketId);
      if (rocket is null)
      {
        _logger.LogError("Rocket {RocketId} not found for flight {FlightId}", flight.RocketId, flightId);
        return new CreateBookingResult.UnexpectedFailure("rocket not found for flight");
      }

      var capacity = rocket.Capacity;
      var currentCount = await _bookingRepository.CountByFlightIdAsync(flightId);

      if (currentCount >= capacity)
      {
        _logger.LogWarning(
          "Rejected booking for flight {FlightId} due to capacity. Current={Current} Capacity={Capacity}",
          flightId,
          currentCount,
          capacity);

        return new CreateBookingResult.Conflict("flight capacity exceeded");
      }

      var newCount = currentCount + 1;

      var (discountRule, discountRate) = DetermineDiscount(newCount, capacity, flight.MinimumPassengers);
      var finalPrice = flight.BasePrice * (1m - discountRate);

      var booking = new Booking
      {
        FlightId = flightId,
        PassengerName = validated.PassengerName,
        PassengerEmail = validated.PassengerEmail,
        FinalPrice = finalPrice
      };

      var created = await _bookingRepository.AddAsync(booking);

      _logger.LogInformation(
        "Computed final price for flight {FlightId} using discount rule {DiscountRule}. FinalPrice={FinalPrice}",
        flightId,
        discountRule,
        finalPrice);

      var fromState = flight.State;
      var toState = fromState;

      if (newCount >= capacity && fromState != FlightState.SOLD_OUT)
      {
        toState = FlightState.SOLD_OUT;
      }
      else if (newCount >= flight.MinimumPassengers && fromState == FlightState.SCHEDULED)
      {
        toState = FlightState.CONFIRMED;
      }

      if (toState != fromState)
      {
        flight.State = toState;

        var updated = await _flightRepository.UpdateAsync(flight);
        if (updated is null)
        {
          _logger.LogError(
            "Failed to update flight {FlightId} from {FromState} to {ToState}",
            flightId,
            fromState,
            toState);

          return new CreateBookingResult.UnexpectedFailure("failed to transition flight state");
        }

        _logger.LogInformation(
          "Flight {FlightId} transitioned from {FromState} to {ToState}. BookingCount={BookingCount} MinimumPassengers={MinimumPassengers} Capacity={Capacity}",
          flightId,
          fromState,
          toState,
          newCount,
          flight.MinimumPassengers,
          capacity);

        if (toState == FlightState.CONFIRMED)
        {
          _logger.LogInformation(
            "Triggering confirmation notification workflow for flight {FlightId}. BookingCount={BookingCount} MinimumPassengers={MinimumPassengers} Capacity={Capacity}",
            flightId,
            newCount,
            flight.MinimumPassengers,
            capacity);
        }
      }

      _logger.LogInformation("Created booking {BookingId} for flight {FlightId}", created.Id, flightId);
      return new CreateBookingResult.Success(created);
    }

    private enum DiscountRule
    {
      LastSeat,
      OneAwayFromMinimumPassengers,
      Standard
    }

    private static (DiscountRule Rule, decimal DiscountRate) DetermineDiscount(
      int newBookingCount,
      int rocketCapacity,
      int minimumPassengers)
    {
      if (newBookingCount == rocketCapacity)
      {
        return (DiscountRule.LastSeat, 0.0m);
      }

      if (newBookingCount == (minimumPassengers - 1))
      {
        return (DiscountRule.OneAwayFromMinimumPassengers, 0.3m);
      }

      return (DiscountRule.Standard, 0.1m);
    }

    /// <summary>
    /// Result type for booking create operations.
    /// </summary>
    public abstract record CreateBookingResult
    {
      /// <summary>
      /// Internal representation for validated input.
      /// </summary>
      public sealed record Validated(string PassengerName, string PassengerEmail) : CreateBookingResult;

      /// <summary>
      /// Validation failure outcome.
      /// </summary>
      public sealed record ValidationFailed(string Error) : CreateBookingResult;

      /// <summary>
      /// Outcome when the referenced flight does not exist.
      /// </summary>
      public sealed record FlightNotFound : CreateBookingResult;

      /// <summary>
      /// Conflict outcome when booking cannot be created (state/capacity).
      /// </summary>
      public sealed record Conflict(string Error) : CreateBookingResult;

      /// <summary>
      /// Unexpected failure outcome.
      /// </summary>
      public sealed record UnexpectedFailure(string Error) : CreateBookingResult;

      /// <summary>
      /// Successful booking creation outcome.
      /// </summary>
      public sealed record Success(Booking Booking) : CreateBookingResult;
    }

    private static CreateBookingResult ValidateDto(string flightId, CreateBookingDto dto)
    {
      if (string.IsNullOrWhiteSpace(flightId))
      {
        return new CreateBookingResult.FlightNotFound();
      }

      if (dto is null)
      {
        return new CreateBookingResult.ValidationFailed("request body is required");
      }

      if (string.IsNullOrWhiteSpace(dto.PassengerName))
      {
        return new CreateBookingResult.ValidationFailed("passengerName is required");
      }

      if (string.IsNullOrWhiteSpace(dto.PassengerEmail))
      {
        return new CreateBookingResult.ValidationFailed("passengerEmail is required");
      }

      return new CreateBookingResult.Validated(dto.PassengerName.Trim(), dto.PassengerEmail.Trim());
    }
  }
}
