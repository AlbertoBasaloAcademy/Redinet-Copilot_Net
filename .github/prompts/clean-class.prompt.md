---
agent: Plan
model: GPT-5.2 (copilot)
description: 'Refactor a single class using basic Clean Code techniques (SRP, cohesion, naming, reduce complexity, manageable size, sensible dependencies).'
argument-hint: 'Provide the class code (and optionally its callers/usages) and any constraints (must-keep public API, performance, exceptions, logging).'
---

# Clean a Class (Clean Code Refactor)

## Task

Refactor **one class** to improve readability, cohesion, and maintainability using basic Clean Code techniques **without changing observable behavior**.

## Context

- [Clean Code Instructions](../instructions/bst_clean-code.instructions.md)
- [C# Guidelines](../instructions/lng_csharp.instructions.md)
- [Layered Architecture Rules](../instructions/layered.instructions.md)
- If applicable:
  - [Minimal APIs Guidelines](../instructions/frm_aspnetcore-minimal-apis.instructions.md)
  - [DI Guidelines](../instructions/lib_dotnet-di.instructions.md)
  - [Logging Guidelines](../instructions/lib_dotnet-logging.instructions.md)
  - [Configuration Guidelines](../instructions/lib_dotnet-configuration.instructions.md)

## Steps

1. **Understand the class contract**
   - Identify public API surface, invariants, side effects, and error behavior.
   - If usages are provided, confirm intended responsibilities and expectations.

2. **Improve cohesion and enforce SRP**
   - Identify distinct responsibilities mixed in the class (validation, orchestration, persistence, mapping, formatting, time, etc.).
   - Split into smaller classes or private helpers only when it clearly reduces responsibility overload.
   - Prefer extracting collaborators behind existing interfaces or introducing a small interface only if it meaningfully improves testability and separation.

3. **Reduce complexity and nesting across methods**
   - Apply guard clauses / early returns inside methods.
   - Replace long boolean expressions with named predicates.
   - Break up very long methods and duplicate blocks.

4. **Improve naming and structure**
   - Rename methods/fields to reflect intent (avoid abbreviations, avoid misleading names).
   - Group members logically (public methods, private helpers, fields) following repo conventions.

5. **Tighten dependencies and constructor arguments**
   - Reduce constructor parameter count where practical (avoid “god constructors”).
   - Prefer:
     - passing a single cohesive dependency (service/repository) rather than many primitives,
     - consolidating related parameters into an existing DTO/model,
     - keeping DI lifetimes and patterns consistent with the repo.
   - Do **not** introduce new external dependencies.

6. **Preserve behavior and keep changes local**
   - Keep the public API stable unless explicitly allowed to change it.
   - If public API changes are unavoidable, update all usages.
   - Maintain exception types/messages and logging semantics unless asked otherwise.
   - Avoid refactoring unrelated code outside the class unless needed for compilation.

## Validation

- The refactor preserves observable behavior
  - [ ] Same outputs for same inputs
  - [ ] Same side effects (persistence, logging, time, I/O)
  - [ ] Same exception behavior where relevant
- The class is cleaner and easier to reason about
  - [ ] Responsibilities are clearer (fewer “mixed concerns”)
  - [ ] Public API is smaller or better organized (where feasible)
  - [ ] High-complexity methods reduced/extracted
  - [ ] Dependencies are sensible and not excessive
- Quality gates
  - [ ] No new dependencies introduced
  - [ ] Existing unit tests pass; add/update tests only if behavior/contract needed clearer coverage
