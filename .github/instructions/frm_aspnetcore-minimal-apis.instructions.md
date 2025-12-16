---
description: 'ASP.NET Core Minimal APIs guidelines (Presentation layer)'
applyTo: '**/*.cs'
---

# ASP.NET Core Minimal APIs

This repo uses ASP.NET Core Minimal APIs (`WebApplication`) with endpoint mapping kept outside `Program.cs`.

## Endpoint placement and structure

- Keep endpoint definitions in `lib/Presentation/*Endpoints.cs` as extension methods.
- `Program.cs` should only:
  - create builder/app
  - register DI
  - map endpoint groups
  - configure middleware (if any)

Recommended shape:

```csharp
public static class RocketEndpoints
{
    public static IEndpointRouteBuilder MapRocketEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/rockets");
        group.MapPost("/", CreateRocket);
        return endpoints;
    }
}
```

## Route groups and consistency

- Use `MapGroup` to keep a consistent prefix per feature.
- Use consistent naming and HTTP semantics:
  - `POST` to create
  - `GET` to read
  - `PUT/PATCH` to modify
  - `DELETE` to remove

## Parameter binding

- Prefer explicit binding when ambiguous (`[FromRoute]`, `[FromQuery]`, `[FromHeader]`, `[FromBody]`, `[FromServices]`).
- Keep handler signatures small; complex inputs should be DTOs.

## Results and status code mapping

- Prefer `TypedResults` over `Results` for clarity and testability.
- Presentation is responsible for mapping Business outcomes to HTTP:
  - `200 OK` / `201 Created` for success
  - `400 BadRequest` for validation failures
  - `404 NotFound` for missing resources
  - `409 Conflict` for domain conflicts (capacity, duplicates, invalid transitions)
  - `500` only for unexpected failures

## Error handling

- Do not throw for expected domain outcomes.
- Keep Business exceptions exceptional; when they occur, log and map to 500.

## Logging

- Use `ILogger<T>` from DI or `app.Logger`.
- Log at `Information` for normal flow milestones; `Warning` for recoverable problems; `Error` for failures.

## Minimal dependencies

- Prefer built-in ASP.NET Core and BCL features.
- Avoid adding new framework dependencies unless the PRD/STRUCTURE explicitly approves them.
