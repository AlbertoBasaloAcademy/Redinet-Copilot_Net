using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using NetAstroBookings.Dtos;
using NetAstroBookings.Business;

namespace NetAstroBookings.Presentation
{
  public static class RocketEndpoints
  {
    public static void MapRocketEndpoints(this WebApplication app)
    {
      app.MapPost("/rockets", async (RocketDto dto, NetAstroBookings.Business.RocketService service) =>
{
  try
  {
    var rocket = await service.CreateAsync(dto);
    if (rocket == null || string.IsNullOrEmpty(rocket.Id))
    {
      return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
    var response = new RocketResponseDto
    {
      Id = rocket.Id,
      Name = rocket.Name,
      Capacity = rocket.Capacity,
      Range = rocket.Range.ToString()
    };

    return Results.Created($"/rockets/{response.Id}", response);
  }
  catch (ArgumentException ex)
  {
    return Results.BadRequest(new { error = ex.Message });
  }
});
    }
  }
}
