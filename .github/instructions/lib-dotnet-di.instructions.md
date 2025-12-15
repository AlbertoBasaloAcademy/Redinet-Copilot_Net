---
description: '.NET built-in dependency injection guidelines (service registration and lifetimes)'
applyTo: '**/*.cs'
---

# .NET Dependency Injection (DI)

Use the built-in `Microsoft.Extensions.DependencyInjection` container.

## Where registrations live

- Put all registrations in `Program.cs` (or small `Add{Feature}` extension methods called from `Program.cs`).
- Do not register services inside Business/Persistence implementations.

## Prefer explicit dependencies

- Business depends on Persistence *abstractions* (interfaces) rather than concrete repositories.
- Presentation depends on Business services.

## Lifetimes (rules of thumb)

- **Singleton**: stateless services and in-memory repositories that are thread-safe.
- **Scoped**: request-scoped state (typical for EF Core DbContext; not used here by default).
- **Transient**: lightweight, stateless helpers.

Avoid lifetime bugs:

- Never inject a scoped service into a singleton.
- Avoid resolving scoped services from the root provider.

## Registration patterns

- Register interfaces for cross-layer boundaries:
  - `IRocketRepository` -> `InMemoryRocketRepository`
  - `IRocketService` -> `RocketService`

- Use “group registration” extension methods to keep `Program.cs` tidy:

```csharp
public static class RocketCompositionRoot
{
    public static IServiceCollection AddRocketFeature(this IServiceCollection services)
    {
        services.AddSingleton<IRocketRepository, InMemoryRocketRepository>();
        services.AddSingleton<IRocketService, RocketService>();
        return services;
    }
}
```

## Constructor selection

- Prefer a single public constructor per type.
- Avoid multiple DI-resolvable constructors (it can be ambiguous).

## Avoid service locator

- Do not pass `IServiceProvider` around.
- Minimal API handlers can receive services as parameters; that’s preferred.
