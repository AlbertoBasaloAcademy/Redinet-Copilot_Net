---
description: '.NET 9 (net9.0) project and platform guidelines'
applyTo: '**/*.{cs,csproj,sln,json}'
---

# .NET 9 (net9.0)

This repository targets **.NET 9** (`net9.0`) and uses the SDK-style project system.

## Target framework

- Keep `TargetFramework` as `net9.0` unless the STRUCTURE explicitly changes.
- Avoid multi-targeting in this workshop repo.

## Project settings

- Prefer `Nullable` enabled and keep nullability warnings clean.
- Prefer `ImplicitUsings` enabled.

## Dependencies (NuGet)

- Prefer platform/BCL features first.
- Don’t introduce new NuGet dependencies unless they are approved in `docs/STRUCTURE.md`.
- If a new dependency is unavoidable, document why and how it impacts layering.

## Compatibility mindset

- Prefer APIs available in net9.0 without conditional compilation.
- Avoid OS-specific behavior unless it’s explicitly required.

## Configuration and logging

- Use the built-in configuration and logging stacks (`Microsoft.Extensions.*`) provided by `WebApplication` defaults.
- Keep configuration in `appsettings*.json` and override via environment variables when needed.
