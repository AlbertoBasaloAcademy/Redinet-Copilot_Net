# Arquitectura y Guía de Estilo — AstrBookings API

Este documento define la arquitectura y la guía de estilo para una API REST de ejemplo llamada AstrBookings (reservas de vuelos espaciales). Está pensada como proyecto didáctico para talleres sobre IA, arquitectura de software y buenas prácticas. El diseño es sencillo y portable entre implementaciones en C#, Java o TypeScript.

**Objetivo:** Proveer una referencia clara y reproducible para construir una API en tres capas (Presentación, Negocio, Persistencia) con buenas prácticas de diseño, pruebas y despliegue.

--

**Resumen de la arquitectura (3 capas)**

- **Capa de Presentación (API / Endpoints):** responsabilidad de exponer HTTP, parsear/serializar DTOs, mapear a llamadas de servicio, manejar códigos HTTP y contratos.
- **Capa de Negocio (Servicios / Dominio):** contiene reglas de negocio, validaciones y orquestación. No debe conocer detalles HTTP ni de almacenamiento.
- **Capa de Persistencia (Repositorios):** abstracción del acceso a datos (in-memory, SQL, NoSQL). Implementaciones concretas se inyectan mediante DI.

--

**Estructura de carpetas sugerida (ejemplo)**

- `src/Presentation` — endpoints, conversiones DTO ↔ dominio, middleware HTTP.
- `src/Business` — servicios, validaciones, entidades del dominio.
- `src/Persistence` — interfaces de repositorio y implementaciones (InMemory, Sql, etc.).
- `src/Dtos` — DTOs de entrada/salida, mapeos simples.
- `tests/Unit` — pruebas unitarias por capa.
- `docs/` — documentación adicional, contratos OpenAPI.

En C# el namespace base podría ser `AstrBookings.<Layer>`; en Java `com.astrbookings.<layer>`; en TypeScript `@astrbookings/<layer>` o carpetas `src/<layer>`.

--

**Contratos HTTP y diseño de endpoints**

- Usa rutas RESTful y plurales: `POST /bookings`, `GET /bookings/{id}`, `GET /bookings?userId=...`.
- Usa verbos HTTP semánticos: `GET` (leer), `POST` (crear), `PUT/PATCH` (actualizar), `DELETE` (eliminar).
- Versionado de API: incluir la versión en la ruta o en encabezados. Ejemplo: `/v1/bookings`.
- Respuestas: devolver siempre JSON con la estructura consistente. Para errores, usar `{ "error": "mensaje", "code": "ERROR_CODE" }`.

--

**DTOs y mapeo**

- Mantén DTOs simples y explícitos. Evita exponer entidades del dominio directamente.
- Validaciones de formato (ej. formatos de fecha, enums) pueden validarse en la capa de presentación para devolver 400 rápidamente, pero las reglas de negocio deben comprobarse en la capa de negocio.
- Ejemplo `BookingDto` (entrada): `{ name: string, passengerCount: number, range: "LEO"|"Moon"|"Mars" }`.

--

**Responsabilidades por capa (más detalle)**

- Presentación:
  - Validación básica de forma (JSON válido, tipos).
  - Traducción DTO → dominio y dominio → DTO.
  - Manejo de códigos HTTP y headers.
  - Autenticación/Autorización a nivel de entrada (ej. middleware).
- Negocio:
  - Reglas y validaciones de negocio (ej. capacidad máxima, disponibilidad).
  - Orquestación entre repositorios y utilidades.
  - Lanzamiento de excepciones de dominio (ej. `DomainException`) que la capa de presentación convertirá a 4xx/5xx.
- Persistencia:
  - CRUD y transacciones.
  - Interfaz (`IBookingRepository` / `BookingRepository`) con métodos asincrónicos.
  - Mapeo entre entidad y almacenamiento.

--

**Manejo de errores y códigos HTTP**

- 200 OK — respuestas exitosas con payload.
- 201 Created — al crear recursos; incluir `Location` con la URL del recurso creado.
- 204 No Content — operaciones exitosas sin payload (ej. eliminación).
- 400 Bad Request — errores de validación simples (formato/DTO).
- 401 Unauthorized — falta de autenticación.
- 403 Forbidden — acceso denegado.
- 404 Not Found — recurso no existe.
- 409 Conflict — conflictos como duplicados.
- 500 Internal Server Error — errores inesperados; registrar detalles pero devolver respuesta genérica.

Errores de negocio: lanzar excepciones específicas en la capa de negocio (ej. `DomainException`) y mapearlas a 4xx en la capa de presentación.

--

**Validación y seguridad**

- Validación: combinar validación declarativa (atributos o esquemas) en la presentación y validación programática en la capa de negocio.
- Autenticación: soportar JWT y/o API keys para los talleres.
- Autorización: checks por rol/claim en endpoints o dentro de servicios.
- Entrada segura: evitar inyección — usar parámetros tipados y ORM/parametrización.
- Rate limiting y CORS: configurar políticas razonables para entornos de demo.

--

**Pruebas**

- Unitarias: pruebas de servicios del dominio y utilidades (mockear repositorios).
- Integración leve: pruebas de endpoints con repositorio in-memory.
- Contrato/OpenAPI: generar y validar el contrato; usar para pruebas y documentación.

--

**Registro, trazas y monitoreo**

- Registrar a nivel de info/errores (en entorno local DEBUG más verboso).
- Añadir correlation id para rastreo (ej. header `X-Correlation-ID`).
- Exponer health checks (`/health`) para integraciones.

--

**Estilo de código y convenciones**

- Nombres: usar nombres claros y descriptivos (`BookingService`, `IBookingRepository`).
- Principios: SRP, DI, TDD cuando proceda.
- Interfaces: definir contratos pequeños y estables para repositorios y servicios.
- Asincronía: preferir APIs async (Task/CompletableFuture/Promise) para IO.

Guías por lenguaje:
- C#: `PascalCase` para tipos y `camelCase` para parámetros; usar `async/await`; namespaces `AstrBookings.<Layer>`.
- Java: `camelCase` para métodos y variables; paquetes `com.astrbookings.<layer>`; usar `CompletableFuture` en APIs async.
- TypeScript: `camelCase` variables, `PascalCase` interfaces/types; usar `async/await`; tipos explícitos para DTOs.

--

**CI / CD y Quality Gates**

- Pipelines: build, lint, run unit tests, generar OpenAPI, publicar artefactos.
- Quality: linters y formateadores (EditorConfig, dotnet-format, ESLint, Checkstyle) y análisis estático opcional.

--

**Guía de ejemplos rápidos**

- Endpoint para crear reserva (semántica):
  - `POST /v1/bookings` — body `BookingDto` → respuesta `201 Created` con `BookingResponseDto`.
- Servicio:
  - `BookingService.Create(dto)` valida, crea `Booking` y llama a `_repo.AddAsync(booking)`.
- Repositorio in-memory para talleres: devuelve ID en formato `b0001`, `b0002`.

--

**Consideraciones pedagógicas**

- Mantener el código legible y con pocos archivos por responsabilidad.
- Evitar dependencias pesadas: preferir in-memory y librerías estándar para que el taller sea reproducible rápidamente.
- Proporcionar ejemplos en C#, Java y TypeScript del mismo flujo (endpoint → servicio → repo) para comparar patrones.

--

**Checklist antes de una entrega de taller**

- [ ] README con instrucciones de build/run.
- [ ] OpenAPI/Swagger actualizado.
- [ ] Pruebas unitarias básicas (>=1 por servicio crítico).
- [ ] Logging mínimo y health-check.
- [ ] Ejemplos de requests curl o Postman.

--

Si quieres, puedo:

- Añadir una plantilla `README.md` y ejemplo `curl` para la API.
- Generar ejemplos concretos en C#, Java o TypeScript que sigan esta guía.

Archivo generado automáticamente por la guía del taller.
