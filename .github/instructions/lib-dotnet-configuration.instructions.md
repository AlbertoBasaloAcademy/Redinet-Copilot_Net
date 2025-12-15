---
description: 'Configuration and appsettings.json guidelines (.NET configuration + options pattern)'
applyTo: '**/*.{cs,json}'
---

# Configuration (.NET)

The app uses `appsettings.json` and `appsettings.Development.json` with the built-in configuration system.

## Principles

- Configuration is **read-only** at runtime; don’t write configuration values programmatically.
- Prefer **strongly typed settings** (options pattern) over stringly-typed key lookups.
- Avoid reading configuration from deep layers; wire configuration at startup and inject into services.

## Environment overrides

- Use `appsettings.{Environment}.json` for per-environment overrides.
- Use `ASPNETCORE_ENVIRONMENT=Development` for local overrides.
- Environment variables override JSON when they are added later in the provider chain.

## Options pattern

- Create a small options class per settings group (e.g., `RocketOptions`).
- Bind configuration sections at startup and inject options into Business/Infrastructure.

Preferred injection forms:

- `IOptions<T>` for static config
- `IOptionsMonitor<T>` for reloadable config (only if needed)

## Configuration key conventions

- Use hierarchical sections in JSON.
- For environment variables, use `__` to represent `:`.
  - Example: `Logging__LogLevel__Default=Information`

## Keep it minimal

- Don’t add new configuration providers (KeyVault, Azure App Config, etc.) unless approved in STRUCTURE.
