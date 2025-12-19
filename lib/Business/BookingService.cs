using System;
using System.Collections.Generic;
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
      if (!TryValidateCreateBookingRequest(flightId, dto, out var validated, out var validationFailure))
      {
        return validationFailure!;
      }

      await using var gate = await _operationGate.AcquireAsync(flightId);

      var flight = await _flightRepository.GetByIdAsync(flightId);
      if (flight is null)
      {
        return new CreateBookingResult.FlightNotFound();
      }

      var notBookable = RejectIfNotBookable(flightId, flight);
      if (notBookable is not null)
      {
        return notBookable;
      }

      var rocket = await _rocketRepository.GetByIdAsync(flight.RocketId);
      if (rocket is null)
      {
        _logger.LogError("Rocket {RocketId} not found for flight {FlightId}", flight.RocketId, flightId);
        return new CreateBookingResult.UnexpectedFailure("rocket not found for flight");
      }

      var capacity = rocket.Capacity;
      var currentCount = await _bookingRepository.CountByFlightIdAsync(flightId);

      var capacityExceeded = RejectIfCapacityExceeded(flightId, currentCount, capacity);
      if (capacityExceeded is not null)
      {
        return capacityExceeded;
      }

      var newCount = currentCount + 1;

      var (discountRule, discountRate) = DetermineDiscount(newCount, capacity, flight.MinimumPassengers);
      var finalPrice = ComputeFinalPrice(flight.BasePrice, discountRate);

      var created = await CreateBookingAsync(flightId, validated!, finalPrice);

      _logger.LogInformation(
        "Computed final price for flight {FlightId} using discount rule {DiscountRule}. FinalPrice={FinalPrice}",
        flightId,
        discountRule,
        finalPrice);

      var transitionFailure = await TryTransitionFlightStateAsync(flightId, flight, newCount, capacity);
      if (transitionFailure is not null)
      {
        return transitionFailure;
      }

      _logger.LogInformation("Created booking {BookingId} for flight {FlightId}", created.Id, flightId);
      return new CreateBookingResult.Success(created);
    }

    private bool TryValidateCreateBookingRequest(
      string flightId,
      CreateBookingDto dto,
      out CreateBookingResult.Validated? validated,
      out CreateBookingResult? validationFailure)
    {
      var validation = ValidateDto(flightId, dto);
      if (validation is CreateBookingResult.Validated ok)
      {
        validated = ok;
        validationFailure = null;
        return true;
      }

      if (validation is CreateBookingResult.ValidationFailed failed)
      {
        _logger.LogWarning("Booking validation failed: {Error}", failed.Error);
      }

      validated = null;
      validationFailure = validation;
      return false;
    }

    private CreateBookingResult? RejectIfNotBookable(string flightId, Flight flight)
    {
      if (flight.State is not (FlightState.CANCELLED or FlightState.SOLD_OUT or FlightState.DONE))
      {
        return null;
      }

      _logger.LogWarning(
        "Rejected booking for flight {FlightId} due to state {State}. Reason={Reason}",
        flightId,
        flight.State,
        "flight is not bookable");

      return new CreateBookingResult.Conflict("flight is not bookable");
    }

    private CreateBookingResult? RejectIfCapacityExceeded(string flightId, int currentCount, int capacity)
    {
      if (currentCount < capacity)
      {
        return null;
      }

      _logger.LogWarning(
        "Rejected booking for flight {FlightId} due to capacity. Current={Current} Capacity={Capacity}",
        flightId,
        currentCount,
        capacity);

      return new CreateBookingResult.Conflict("flight capacity exceeded");
    }

    private static decimal ComputeFinalPrice(decimal basePrice, decimal discountRate)
    {
      return basePrice * (1m - discountRate);
    }

    private async Task<Booking> CreateBookingAsync(
      string flightId,
      CreateBookingResult.Validated validated,
      decimal finalPrice)
    {
      var booking = new Booking
      {
        FlightId = flightId,
        PassengerName = validated.PassengerName,
        PassengerEmail = validated.PassengerEmail,
        FinalPrice = finalPrice
      };

      return await _bookingRepository.AddAsync(booking);
    }

    private static FlightState DetermineNextState(
      FlightState fromState,
      int newCount,
      int capacity,
      int minimumPassengers)
    {
      if (newCount >= capacity && fromState != FlightState.SOLD_OUT)
      {
        return FlightState.SOLD_OUT;
      }

      if (newCount >= minimumPassengers && fromState == FlightState.SCHEDULED)
      {
        return FlightState.CONFIRMED;
      }

      return fromState;
    }

    private async Task<CreateBookingResult?> TryTransitionFlightStateAsync(
      string flightId,
      Flight flight,
      int newCount,
      int capacity)
    {
      var fromState = flight.State;
      var toState = DetermineNextState(fromState, newCount, capacity, flight.MinimumPassengers);

      if (toState == fromState)
      {
        return null;
      }

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

      return null;
    }

    /// <summary>
    /// Lists all bookings for a flight.
    /// Returns <see cref="ListBookingsResult.FlightNotFound"/> when the flight does not exist.
    /// </summary>
    /// <param name="flightId">Flight identifier.</param>
    /// <returns>A result describing either success or not found.</returns>
    public async Task<ListBookingsResult> ListByFlightIdAsync(string flightId)
    {
      if (string.IsNullOrWhiteSpace(flightId))
      {
        _logger.LogWarning("Rejected list bookings request because flightId is empty");
        return new ListBookingsResult.FlightNotFound();
      }

      var flight = await _flightRepository.GetByIdAsync(flightId);
      if (flight is null)
      {
        _logger.LogWarning("Flight {FlightId} not found while listing bookings", flightId);
        return new ListBookingsResult.FlightNotFound();
      }

      var bookings = await _bookingRepository.ListByFlightIdAsync(flightId);

      _logger.LogInformation(
        "Listed bookings for flight {FlightId}. Count={BookingCount}",
        flightId,
        bookings.Count);

      return new ListBookingsResult.Success(bookings);
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

    /// <summary>
    /// Result type for booking list operations.
    /// </summary>
    public abstract record ListBookingsResult
    {
      /// <summary>
      /// Outcome when the referenced flight does not exist.
      /// </summary>
      public sealed record FlightNotFound : ListBookingsResult;

      /// <summary>
      /// Successful booking list outcome.
      /// </summary>
      public sealed record Success(IReadOnlyList<Booking> Bookings) : ListBookingsResult;

      /// <summary>
      /// Unexpected failure outcome.
      /// </summary>
      public sealed record UnexpectedFailure(string Error) : ListBookingsResult;
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
