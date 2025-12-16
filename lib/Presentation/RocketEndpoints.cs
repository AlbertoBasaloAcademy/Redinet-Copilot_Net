using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetAstroBookings.Dtos;
using NetAstroBookings.Business;

namespace NetAstroBookings.Presentation
{
  public static class RocketEndpoints
  {
    /// <summary>
    /// Maps HTTP endpoints related to rocket management.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder used to register routes.</param>
    /// <returns>The same <paramref name="endpoints"/> instance.</returns>
    public static IEndpointRouteBuilder MapRocketEndpoints(this IEndpointRouteBuilder endpoints)
    {
      var group = endpoints.MapGroup("/rockets");

      group.MapPost(string.Empty, CreateRocket);
      group.MapGet(string.Empty, ListRockets);
      group.MapGet("/{id}", GetRocketById);

      return endpoints;
    }

    private static async Task<IResult> CreateRocket(
      [FromBody] RocketDto dto,
      [FromServices] RocketService service)
    {
      var result = await service.CreateAsync(dto);
      return result switch
      {
        RocketService.CreateRocketResult.Success success =>
          TypedResults.Created($"/rockets/{success.Rocket.Id}", Map(success.Rocket)),

        RocketService.CreateRocketResult.ValidationFailed invalid =>
          TypedResults.BadRequest(new { error = invalid.Error }),

        _ => TypedResults.StatusCode(StatusCodes.Status500InternalServerError)
      };
    }

    private static async Task<IResult> ListRockets(
      [FromServices] RocketService service)
    {
      IReadOnlyList<NetAstroBookings.Models.Rocket> rockets = await service.ListAsync();
      var response = rockets.Select(Map).ToList();
      return TypedResults.Ok(response);
    }

    private static async Task<IResult> GetRocketById(
      [FromRoute] string id,
      [FromServices] RocketService service)
    {
      var result = await service.GetByIdAsync(id);
      return result switch
      {
        RocketService.GetRocketResult.Found found => TypedResults.Ok(Map(found.Rocket)),
        RocketService.GetRocketResult.NotFound => TypedResults.NotFound(),
        _ => TypedResults.StatusCode(StatusCodes.Status500InternalServerError)
      };
    }

    private static RocketResponseDto Map(NetAstroBookings.Models.Rocket rocket)
    {
      return new RocketResponseDto
      {
        Id = rocket.Id ?? string.Empty,
        Name = rocket.Name,
        Capacity = rocket.Capacity,
        Speed = rocket.Speed,
        Range = rocket.Range.ToString()
      };
    }
  }
}
