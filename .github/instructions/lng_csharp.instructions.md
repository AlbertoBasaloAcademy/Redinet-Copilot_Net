---
description: 'C# 12 language guidelines for this repo (net9.0)'
applyTo: '**/*.cs'
---

# C# 12 guidelines (net9.0)

Use modern, idiomatic C# that stays easy to read in a workshop setting.

## Defaults

- Assume `nullable` is enabled; keep nullability warnings at zero.
- Prefer explicit domain types over primitives when it improves clarity.
- Prefer small, composable methods and single-purpose types.

## Naming and organization

- Use `PascalCase` for public types/members; `camelCase` for locals/parameters.
- Use file-scoped namespaces.
- Keep one public type per file when practical.
- Keep feature code in `lib/Presentation`, `lib/Business`, `lib/Persistence`, `lib/Models`, `lib/Dtos`.

## Records, DTOs, and immutability

- Prefer `record` / `record struct` for request/response DTOs.
- Prefer immutable domain models (constructor + `init`-only properties) unless mutation is required.
- Avoid exposing mutable collections; return `IReadOnlyList<T>` / `IReadOnlyDictionary<TKey,TValue>` when possible.

## Exceptions and validation

- Don’t use exceptions for expected control flow (validation, not-found, conflicts).
- Use guard clauses early:
  - `ArgumentNullException.ThrowIfNull(value)`
  - `ArgumentException.ThrowIfNullOrEmpty(str)` when available
- Prefer returning explicit outcomes from Business to Presentation (e.g., success vs conflict) instead of throwing.

## Async and cancellation

- Use `async`/`await` for I/O-like work; keep CPU-only paths synchronous.
- Accept `CancellationToken` in public async methods where requests can cancel.
- Avoid `Task.Run` in the web app.

## Logging friendliness

- Use structured logging placeholders rather than string interpolation.
- Keep log messages short and stable; avoid logging secrets or personal data.

## Modern syntax (use judiciously)

- Primary constructors are allowed when they improve clarity, especially for DI:
  
  ```csharp
  public sealed class RocketService(IRocketRepository repository)
  {
      // ...
  }
  ```

- Use pattern matching for clear branching.
- Use `switch` expressions where they reduce boilerplate.

## Keep the learning curve low

- Avoid overly clever LINQ chains; prefer readable loops where appropriate.
- Avoid metaprogramming-heavy techniques unless required.
