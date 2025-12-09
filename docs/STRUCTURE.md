## Estructura y cómo compilar/ejecutar

Resumen rápido
- **Proyecto:** `Redinet-Copilot_Net`
- **Target framework:** `net9.0` (según la salida en `bin/` y `obj/`).

Pasos para compilar y ejecutar (Windows, usando `bash.exe` o WSL)

1. Abrir una terminal en el directorio raíz del repo:

```bash
cd /c/code/live/Redinet-Copilot_Net
```

2. Restaurar paquetes (opcional, `dotnet build` lo hace automáticamente si falta):

```bash
dotnet restore
```

3. Compilar el proyecto:

```bash
dotnet build -c Debug
```

4. Ejecutar la aplicación (desde la carpeta raíz):

```bash
dotnet run --project Redinet-Copilot_Net.csproj
```

5. Probar el endpoint POST `/rockets` (ejemplo con `curl`):

```bash
curl -i -X POST http://localhost:5000/rockets \
  -H "Content-Type: application/json" \
  -d '{"name":"Explorer","capacity":4,"range":"LEO"}'
```

Si la aplicación está escuchando en otro puerto (por ejemplo Kestrel asigna automáticamente), consulte la salida del `dotnet run` para la URL.

Descripción de la solución

Esta solución es una API mínima ASP.NET que permite crear cohetes (rockets). La lógica está dividida en capas sencillas dentro de la carpeta `lib/`:

- `lib/Models` : modelos de dominio. Contiene `Rocket` y el enum `RocketRange`.
- `lib/Dtos` : objetos de transferencia (DTOs) para entrada y salida (`RocketDto`, `RocketResponseDto`).
- `lib/Persistence` : repositorio en memoria (`InMemoryRocketRepository`) que asigna un Id secuencial al crear cohetes.
- `lib/Business` : lógica de negocio (`RocketService`) que valida DTOs y usa el repositorio.
- `lib/Presentation` : mapeo de endpoints HTTP (`RocketEndpoints`) que exponen el endpoint `POST /rockets`.

Ficheros importantes en la raíz

- `Program.cs` : arranque de la aplicación, configuración de DI y llamada a los mapeos de endpoints.
- `Redinet-Copilot_Net.csproj` : archivo de proyecto .NET.
- `appsettings.json` / `appsettings.Development.json` : configuración de la aplicación.

Notas sobre el comportamiento

- El repositorio en memoria usa `ConcurrentDictionary` y genera Ids en formato `r0001`, `r0002`, ...
- `RocketService` valida: `Name` no vacío, `Capacity` en rango (1..10), y `Range` debe poder parsearse al enum `RocketRange` (acepta "LEO", "Moon", "Mars", case-insensitive).
- Si la creación tiene éxito, el endpoint devuelve `201 Created` con el `RocketResponseDto`.

Cómo extender o probar localmente

- Para persistencia real, sustituir `InMemoryRocketRepository` por una implementación que use base de datos y registrar en DI.
- Añadir más endpoints (GET, PUT, DELETE) en `lib/Presentation/RocketEndpoints.cs`.

Contacto y próximos pasos

- Si quieres, puedo: ejecutar `dotnet build` aquí, añadir más endpoints, o crear pruebas unitarias para `RocketService`.
