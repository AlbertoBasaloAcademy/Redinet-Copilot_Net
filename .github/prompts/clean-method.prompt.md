---
agent: Plan
model: GPT-5.2 (copilot)
description: 'Refactor a single method using basic Clean Code techniques (reduce nesting, early returns, shorter length, fewer arguments).'
argument-hint: 'Provide the method code (and optionally its callers/usages) and any constraints (must-keep signature, performance, exceptions, logging).'
---

# Clean a Method (Clean Code Refactor)

## Task

Refactor **one method** to improve readability and maintainability using basic Clean Code techniques **without changing its observable behavior**.

## Context

- [Clean Code Instructions](../instructions/bst_clean-code.instructions.md)
- [C# Guidelines](../instructions/lng_csharp.instructions.md)
- [Layered Architecture Rules](../instructions/layered.instructions.md)
- If applicable:
  - [Minimal APIs Guidelines](../instructions/frm_aspnetcore-minimal-apis.instructions.md)
  - [Logging Guidelines](../instructions/lib_dotnet-logging.instructions.md)

## Steps

1. **Understand intent and behavior**
   - Identify the method’s single responsibility, inputs/outputs, side effects, and error behavior.
   - If callers/usages are provided, confirm how the method is expected to behave.

2. **Reduce nesting with guard clauses and early returns**
   - Replace deep `if/else` and nested loops with early returns.
   - Prefer “happy path” flow; push exceptional/edge cases to the top.

3. **Shorten and simplify control flow**
   - Simplify boolean logic and conditions (remove redundancy, use clearer predicates).
   - Prefer `switch`/pattern matching when it clarifies intent.
   - Extract complex expressions into well-named local variables.

4. **Keep the method short and focused**
   - Extract well-named private helper methods for distinct sub-tasks.
   - Avoid mixing validation, business rules, persistence, and formatting concerns.

5. **Reduce argument count (carefully)**
   - Prefer keeping the public signature stable unless explicitly allowed to change it.
   - If the method has many parameters, consider:
     - passing an existing domain model/DTO already present in the codebase, or
     - introducing a simple parameter object only if it is clearly justified and used coherently.
   - Do **not** add new dependencies or large abstractions.

6. **Preserve correctness and existing conventions**
   - Maintain exception types/messages and logging semantics unless asked otherwise.
   - Keep changes minimal and local; do not refactor unrelated code.
   - If signature changes are necessary, update all usages.

## Validation

- The refactored method compiles and preserves observable behavior
  - [ ] Same outputs for same inputs (including edge cases)
  - [ ] Same side effects (I/O, persistence, logging)
  - [ ] Same exception behavior where relevant
- Readability improvements are evident
  - [ ] Nesting depth reduced (aim ≤ 2)
  - [ ] Method length reduced (aim ≤ ~30 logical lines)
  - [ ] Argument list reduced or justified (aim ≤ 4, where practical)
- Quality gates
  - [ ] No new dependencies introduced
  - [ ] Existing unit tests pass; add/update tests only if behavior/contract needed clearer coverage
