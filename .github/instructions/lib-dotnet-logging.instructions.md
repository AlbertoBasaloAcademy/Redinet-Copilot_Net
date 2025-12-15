---
description: 'Logging guidelines (Microsoft.Extensions.Logging)'
applyTo: '**/*.cs'
---

# Logging (Microsoft.Extensions.Logging)

Use structured logging via `ILogger<T>` from DI.

## How to log

- Prefer `ILogger<T>` injected into services/endpoints.
- Prefer message templates over string interpolation:

```csharp
logger.LogInformation("Created rocket {RocketId}", rocket.Id);
```

## Log levels

- `Information`: normal flow milestones (created, confirmed, cancelled).
- `Warning`: validation failures, conflicts, threshold not met, unexpected but handled conditions.
- `Error`: unhandled exceptions or failed operations.

Avoid `Trace`/`Debug` unless actively diagnosing.

## What not to log

- Don’t log secrets, tokens, or personal data.
- Don’t log entire request/response bodies by default.

## Exceptions

- When catching an exception you’ll rethrow or convert to 500, log it with the exception parameter:

```csharp
logger.LogError(ex, "Failed to create rocket");
```

## Scopes (optional)

- Use scopes to correlate logs across a request or operation if needed.
- Keep scope payloads small and stable.

## Consistency

- Keep message templates stable (don’t build templates dynamically).
- Use consistent placeholder names (`RocketId`, `FlightId`, `BookingId`).
