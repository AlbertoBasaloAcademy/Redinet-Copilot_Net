---
description: 'dotnet CLI workflow (build/run/format/test)'
applyTo: '**/*'
---

# dotnet CLI workflow

Use the .NET SDK CLI for local development.

## Common commands

- Build: `dotnet build`
- Run: `dotnet run`
- Run with environment:
  - PowerShell: `$env:ASPNETCORE_ENVIRONMENT = 'Development'; dotnet run`
- Run with URLs: `dotnet run --urls="https://localhost:7777"`

## Formatting

- Use `dotnet format` to keep formatting consistent.
- Prefer formatting before opening PRs.

## Tests

- When tests exist: `dotnet test`

## Dependency hygiene

- Prefer built-in platform features.
- Don’t add new NuGet packages unless they are approved in `docs/STRUCTURE.md`.
