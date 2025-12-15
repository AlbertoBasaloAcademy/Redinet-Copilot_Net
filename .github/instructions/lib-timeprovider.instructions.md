---
description: 'Deterministic time guidelines (TimeProvider / ITimeProvider abstraction)'
applyTo: '**/*.cs'
---

# Deterministic time

Time-based rules (like automatic cancellation windows) should be deterministic and testable.

## Rule

- Never call `DateTime.UtcNow` directly inside Business logic.

## Preferred approach

- Use .NET `TimeProvider` (available in modern .NET) or an app-defined interface (e.g., `ITimeProvider`) injected via DI.
- In production, use `TimeProvider.System`.
- In tests/workshops, use a fake/frozen provider to control “now”.

## Example shape

```csharp
public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

## Usage

- Business services accept `ITimeProvider` in the constructor.
- Presentation should not compute domain time decisions; it should delegate to Business.
