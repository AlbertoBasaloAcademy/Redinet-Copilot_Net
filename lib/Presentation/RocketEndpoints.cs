using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using NetAstroBookings.Dtos;
using NetAstroBookings.Business;

namespace NetAstroBookings.Presentation
{
  public static class RocketEndpoints
  {
    /// <summary>
    /// Extensión que mapea los endpoints HTTP relacionados con cohetes en la aplicación.
    /// Registra el endpoint POST /rockets que crea un nuevo cohete.
    /// </summary>
    /// <param name="app">Instancia de <see cref="WebApplication"/> donde se añaden las rutas.</param>
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
