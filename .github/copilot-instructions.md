<!-- Copilot / AI agent instructions for the Redinet-Copilot_Net repository -->
# Quick Agent Guide — Redinet-Copilot_Net

This file contains concise, actionable guidance for AI coding agents working on this repository. Focus on discoverable patterns and project-specific workflows.

**Big Picture**
- **Project type:** Minimal ASP.NET Core Web API targeting `net9.0` (see `bin/` and `obj/`).
- **Purpose:** Small training/sample API that exposes `POST /rockets` to create `Rocket` domain objects.
- **Layering:** Code is organized under `lib/` by responsibility: `Models`, `Dtos`, `Persistence`, `Business`, `Presentation`.

**Key Files / Entry Points**
- **`Program.cs`**: app startup, DI registrations, and `app.MapRocketEndpoints()` call.
- **`lib/Presentation/RocketEndpoints.cs`**: Minimal-API route mapping (extension method `MapRocketEndpoints`). Copy patterns from this file when adding endpoints.
- **`lib/Business/RocketService.cs`**: Business validation and orchestration; throws `ArgumentException` for invalid DTOs.
- **`lib/Persistence/InMemoryRocketRepository.cs`**: In-memory repository that returns IDs in the format `r0001`, `r0002`, ... — used in examples and tests.
- **`lib/Models/Rocket.cs` / `lib/Dtos/*.cs`**: Domain and DTO shapes. `RocketDto.Range` expects enum names (case-insensitive).

**Conventions & Patterns (project-specific)**
- **Namespaces:** Use `NetAstroBookings.<Layer>` (see existing files). Keep the same casing and folder→namespace mapping.
- **Layer responsibilities:**
  - `Presentation`: minimal API mapping; map HTTP-level concerns and convert DTOs ↔ domain.
  - `Business`: validate DTOs, create domain objects, call repository.
  - `Persistence`: CRUD; current impl is in-memory and synchronous-friendly (returns Tasks). Replace with a DB-backed implementation by honoring the same method signatures.
- **Error handling:** `RocketService.CreateAsync` raises `ArgumentException` for validation errors; endpoints catch this and return `400 Bad Request` with `{ error: ... }` payload. Follow this pattern when adding validations.
- **DI rules:** `InMemoryRocketRepository` is registered as `Singleton`, `RocketService` as `Scoped` (see `Program.cs`). Keep intended lifetimes when replacing implementations.

**APIs & Data Flow Example**
- Request flow for `POST /rockets`:
  1. Minimal API model-binding maps JSON into `RocketDto`.
  2. `RocketEndpoints` calls `RocketService.CreateAsync(dto)`.
  3. `RocketService` validates, constructs `Rocket` and calls `_repository.AddAsync(rocket)`.
  4. Repository assigns `Id` like `r0001` and returns the persisted `Rocket`.
  5. Endpoint returns `201 Created` with a `RocketResponseDto`.
- Validation specifics: `Name` required; `Capacity` must be 1..10; `Range` must parse to `RocketRange` (LEO, Moon, Mars).

**Build / Run / Test**
- Build: `dotnet build -c Debug` (root)
- Run: `dotnet run --project Redinet-Copilot_Net.csproj` (root)
- Quick API test (example from README):
  - `curl -i -X POST http://localhost:5000/rockets -H "Content-Type: application/json" -d '{"name":"Explorer","capacity":4,"range":"LEO"}'`
- Note: Kestrel may choose a different port; inspect `dotnet run` output.

**When editing code**
- Keep small, focused changes per PR. Mirror existing folder and namespace layout.
- When adding endpoints:
  - Add mapping inside `lib/Presentation/RocketEndpoints.cs` or create a new file under `Presentation` and call its map extension from `Program.cs`.
  - Convert DTOs in presentation layer; do not leak HTTP types into `Business`.
- When changing persistence, preserve repository method signatures so `RocketService` (and any callers) do not require changes.

**Examples & snippets to follow project style**
- Registering a new persistence implementation (example):
  - `builder.Services.AddSingleton<NetAstroBookings.Persistence.InMemoryRocketRepository>(); // existing`
  - Replace with: `builder.Services.AddSingleton<NetAstroBookings.Persistence.IRocketRepository, SqlRocketRepository>();`
- Adding a GET endpoint (pattern):
  - Add a `MapGet` in `MapRocketEndpoints` that uses `RocketService` and returns `Results.Ok(...)` or `Results.NotFound()`.

**Notes / Gotchas**
- There are no unit tests in the repository; write small unit tests for `RocketService` when adding validation changes.
- The repo uses `required` property (`RocketResponseDto.Id`) and nullable reference types in models — follow types accordingly.
- The project is intentionally minimal and educational; prefer clarity over clever optimizations.

---
If any area above is unclear or you want more examples (e.g., adding full CRUD, introducing EF Core persistence, or unit-tests), tell me which part to expand and I will update this file.
