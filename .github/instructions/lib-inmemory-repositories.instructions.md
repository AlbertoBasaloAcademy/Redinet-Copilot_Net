---
description: 'In-memory persistence guidelines (ConcurrentDictionary repositories)'
applyTo: '**/*.cs'
---

# In-memory persistence

Persistence is in-memory by default to keep local execution deterministic and dependency-free.

## Thread safety

- Treat repositories as shared state; make them thread-safe.
- Prefer `ConcurrentDictionary<TKey,TValue>` for keyed storage.
- Avoid returning mutable references that callers can modify.

## Deterministic behavior

- Prefer deterministic IDs (for workshop/test repeatability).
  - Example approach: `long` IDs using `Interlocked.Increment`.
- Ensure repository operations have deterministic outcomes for the same input.

## API design

- Repository interfaces should be small and feature-focused.
- Prefer methods that express intent:
  - `TryCreate(...)` returning success/conflict
  - `GetById(...)` returning nullable

## Conflict handling

- Use atomic operations (`TryAdd`, `TryUpdate`, `TryRemove`) where possible.
- Map persistence conflicts to Business outcomes (and then to HTTP `409`).

## Data boundaries

- Keep repositories in `lib/Persistence`.
- Keep domain models in `lib/Models` and DTOs in `lib/Dtos`.
- Persistence should not depend on Presentation.
