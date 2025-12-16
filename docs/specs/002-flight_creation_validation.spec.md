# 002-flight_creation_validation Specification

## 1. 👔 Problem Specification

The API must allow workshop users and API consumers to create Flights linked to a Rocket. Flights represent scheduled launches that can later be booked, validated against time and pricing rules.

### Create Flight

- **As a** trainer/student (API operator)
- **I want to** create a Flight linked to an existing Rocket
- **So that** I can set up demo data for querying flights and creating bookings

### Validate Flight Input

- **As a** client app (API consumer)
- **I want to** receive clear validation errors when creating a Flight
- **So that** I can correct input and avoid invalid flight state

### Default Minimum Passengers

- **As a** client app (API consumer)
- **I want to** omit `minimumPassengers` and get a sensible default
- **So that** I can create flights with minimal input for workshop demos

## 2. 🧑‍💻 Solution Overview

Implement Flight creation as a small, workshop-friendly REST feature following the existing 3-layer structure (Presentation → Business → Persistence) and using in-memory storage.

### Data Models

- **Domain model**: `Flight`
  - `Id` (string, assigned by repository; deterministic sequential IDs like `f0001`)
  - `RocketId` (string, required)
  - `LaunchDate` (DateTimeOffset, required; must be in the future)
  - `BasePrice` (decimal, required; must be > 0)
  - `MinimumPassengers` (int, optional on input; defaults to 5)
  - `State` (enum; set to `SCHEDULED` on creation)

- **Domain enum**: `FlightState`
  - `SCHEDULED`, `CONFIRMED`, `SOLD_OUT`, `CANCELLED`, `DONE`

- **DTOs**
  - Create request: `CreateFlightDto` (or `FlightDto`)
    - `rocketId`, `launchDate`, `basePrice`, optional `minimumPassengers`
  - Response: `FlightResponseDto`
    - includes `id`, `rocketId`, `launchDate`, `basePrice`, `minimumPassengers`, `state`

### Software Components

- **Presentation** (`lib/Presentation`)
  - `FlightEndpoints` maps the route for creating flights and converts DTOs ↔ domain models.
  - Maps Business outcomes to HTTP status codes (`201`, `400`, `404`).

- **Business** (`lib/Business`)
  - `FlightService` validates flight input and orchestrates repository calls.
  - Uses `TimeProvider` to validate `launchDate` is in the future.
  - Validates referenced Rocket exists (via `IRocketRepository`) before creating a Flight.
  - Returns explicit results for expected outcomes (validation failure, rocket not found).

- **Persistence** (`lib/Persistence`)
  - `IFlightRepository` abstraction supports add and get-by-id (optional for later features).
  - `InMemoryFlightRepository` stores flights in a thread-safe structure.

### User Interface

HTTP/JSON endpoints (Minimal APIs):

- `POST /flights` creates a Flight.

### Aspects

- **Validation**:
  - Enforce PRD rules: `launchDate` is in the future, `basePrice > 0`, `minimumPassengers` defaults to `5` when omitted.
  - Set `state` to `SCHEDULED` on creation.
  - Validate the referenced Rocket exists.

- **Error handling**:
  - Return `400 Bad Request` for validation errors.
  - Return `404 Not Found` when the referenced Rocket does not exist.

- **Deterministic time**:
  - Business logic uses injected `TimeProvider` to evaluate "future" dates.

- **Logging**:
  - Log flight creation at `Information`.
  - Log validation failures at `Warning`.

- **Security**:
  - No authentication/authorization for this workshop version.

- **Concurrency**:
  - Repository operations are safe for concurrent requests.

## 3. 🧑‍⚖️ Acceptance Criteria

- [ ] WHEN a client submits `POST /flights` with a `launchDate` that is not in the future, the system SHALL return `400 Bad Request`.
- [ ] WHEN a client submits `POST /flights` with `basePrice` less than or equal to `0`, the system SHALL return `400 Bad Request`.
- [ ] IF a client submits `POST /flights` referencing a non-existent `rocketId`, THEN the system SHALL return `404 Not Found`.

- [ ] WHEN a client submits `POST /flights` without `minimumPassengers`, the system SHALL create the Flight with `minimumPassengers` set to `5`.
- [ ] WHEN a client submits `POST /flights` with valid input, the system SHALL create a Flight with `state` set to `SCHEDULED`.
- [ ] WHEN a client submits `POST /flights` with valid input, the system SHALL return `201 Created` with the created Flight representation.

> End of Feature Specification for 002-flight_creation_validation, last updated 2025-12-16.
