using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using NetAstroBookings.Business;
using NetAstroBookings.Dtos;

namespace NetAstroBookings.Presentation
{
  /// <summary>
  /// Minimal API endpoints for booking creation.
  /// </summary>
  public static class BookingEndpoints
  {
    /// <summary>
    /// Maps HTTP endpoints related to booking management.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder used to register routes.</param>
    /// <returns>The same <paramref name="endpoints"/> instance.</returns>
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder endpoints)
    {
      var group = endpoints.MapGroup("/flights");

      group.MapPost("/{flightId}/bookings", CreateBooking);
      group.MapGet("/{flightId}/bookings", ListBookings);

      return endpoints;
    }

    private static async Task<IResult> ListBookings(
      [FromRoute] string flightId,
      [FromServices] BookingService service)
    {
      var result = await service.ListByFlightIdAsync(flightId);
      return result switch
      {
        BookingService.ListBookingsResult.Success success =>
          TypedResults.Ok(success.Bookings.Select(Map).ToList()),

        BookingService.ListBookingsResult.FlightNotFound =>
          TypedResults.NotFound(),

        _ => TypedResults.StatusCode(StatusCodes.Status500InternalServerError)
      };
    }

    private static async Task<IResult> CreateBooking(
      [FromRoute] string flightId,
      [FromBody] CreateBookingDto dto,
      [FromServices] BookingService service)
    {
      var result = await service.CreateAsync(flightId, dto);
      return result switch
      {
        BookingService.CreateBookingResult.Success success =>
          TypedResults.Created(
            $"/flights/{success.Booking.FlightId}/bookings/{success.Booking.Id}",
            Map(success.Booking)),

        BookingService.CreateBookingResult.ValidationFailed invalid =>
          TypedResults.BadRequest(new { error = invalid.Error }),

        BookingService.CreateBookingResult.FlightNotFound =>
          TypedResults.NotFound(),

        BookingService.CreateBookingResult.Conflict conflict =>
          TypedResults.Conflict(new { error = conflict.Error }),

        _ => TypedResults.StatusCode(StatusCodes.Status500InternalServerError)
      };
    }

    private static BookingResponseDto Map(NetAstroBookings.Models.Booking booking)
    {
      return new BookingResponseDto
      {
        Id = booking.Id ?? string.Empty,
        FlightId = booking.FlightId,
        PassengerName = booking.PassengerName,
        PassengerEmail = booking.PassengerEmail,
        FinalPrice = booking.FinalPrice
      };
    }
  }
}
