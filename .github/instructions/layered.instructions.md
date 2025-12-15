---
description: 'Layered architecture rules (Presentation/Business/Persistence)'
applyTo: '**/*.cs'
---

# Arquitectura en capas (3 capas)

Este proyecto sigue una arquitectura en capas para separar responsabilidades y mantener el dominio fácil de evolucionar.

## Capas

1. **Presentation (HTTP)**
	- Endpoints Minimal API, DTO mapping y mapeo de status codes.
	- No contiene reglas de negocio.
	- Ubicación: `lib/Presentation/`

2. **Business (dominio y orquestación)**
	- Reglas del dominio (validación, transiciones de estado, pricing, etc.).
	- Orquesta llamadas a repositorios a través de **abstracciones**.
	- Ubicación: `lib/Business/`

3. **Persistence (almacenamiento)**
	- Implementaciones de repositorios (por defecto: en memoria).
	- No conoce HTTP ni DTOs.
	- Ubicación: `lib/Persistence/`

## Dirección de dependencias (regla clave)

- Presentation → Business → Persistence (abstracciones)
- Persistence **no** depende de Presentation.

## Contratos

- DTOs en `lib/Dtos/` (request/response).
- Modelos de dominio en `lib/Models/`.
- Presentation convierte DTOs ⇄ modelos y traduce resultados del dominio a HTTP.

## Manejo de errores

- No uses excepciones para flujos esperados (validación, not-found, conflicts).
- Business retorna resultados explícitos; Presentation mapea a `400/404/409`.