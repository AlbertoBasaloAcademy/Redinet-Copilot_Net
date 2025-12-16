using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using NetAstroBookings.Business;
using NetAstroBookings.Dtos;

namespace NetAstroBookings.Presentation
{
  /// <summary>
  /// Minimal API endpoints for flight creation.
  /// </summary>
  public static class FlightEndpoints
  {
    /// <summary>
    /// Maps HTTP endpoints related to flight management.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder used to register routes.</param>
    /// <returns>The same <paramref name="endpoints"/> instance.</returns>
    public static IEndpointRouteBuilder MapFlightEndpoints(this IEndpointRouteBuilder endpoints)
    {
      var group = endpoints.MapGroup("/flights");

      group.MapPost(string.Empty, CreateFlight);

      return endpoints;
    }

    private static async Task<IResult> CreateFlight(
      [FromBody] CreateFlightDto dto,
      [FromServices] FlightService service)
    {
      var result = await service.CreateAsync(dto);
      return result switch
      {
        FlightService.CreateFlightResult.Success success =>
          TypedResults.Created($"/flights/{success.Flight.Id}", Map(success.Flight)),

        FlightService.CreateFlightResult.ValidationFailed invalid =>
          TypedResults.BadRequest(new { error = invalid.Error }),

        FlightService.CreateFlightResult.RocketNotFound =>
          TypedResults.NotFound(),

        _ => TypedResults.StatusCode(StatusCodes.Status500InternalServerError)
      };
    }

    private static FlightResponseDto Map(NetAstroBookings.Models.Flight flight)
    {
      return new FlightResponseDto
      {
        Id = flight.Id ?? string.Empty,
        RocketId = flight.RocketId,
        LaunchDate = flight.LaunchDate,
        BasePrice = flight.BasePrice,
        MinimumPassengers = flight.MinimumPassengers,
        State = flight.State.ToString()
      };
    }
  }
}
